﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Research.Kinect.Nui;
using KinectNui = Microsoft.Research.Kinect.Nui;
using System.Data;
using System.Collections;

namespace Microsoft.Samples.Kinect.WpfViewers
{
    /// <summary>
    /// Interaction logic for KinectDiagnosticViewer.xaml
    /// </summary>
    public partial class KinectDiagnosticViewer : UserControl
    {
        #region Public API
        public KinectDiagnosticViewer()
        {
            InitializeComponent();
        }

        public RuntimeOptions RuntimeOptions { get; private set; }

        public void ReInitRuntime()
        {
            // Will call Uninitialize followed by Initialize.
            this.Kinect = this.Kinect;
        }

        public KinectNui.Runtime Kinect
        {
            get { return _Kinect; }
            set
            {
                //Clean up existing runtime if we are being set to null, or a new Runtime.
                if (_Kinect != null)
                {
                    kinectColorViewer.Kinect = null;
                    kinectDepthViewer.Kinect = null;                    
                    _Kinect.SkeletonFrameReady -= new EventHandler<SkeletonFrameReadyEventArgs>(nui_SkeletonFrameReady);
                    _Kinect.Uninitialize();
                }

                _Kinect = value;

                if (_Kinect != null)
                {
                    InitRuntime();

                    kinectColorViewer.Kinect = _Kinect;

                    kinectDepthViewer.RuntimeOptions = RuntimeOptions;
                    kinectDepthViewer.Kinect = _Kinect;

                    _Kinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(nui_SkeletonFrameReady);

                    UpdateUi();
                }
            }
        }

        //Status and InstanceIndex can change. Those properties in the Runtime should be made to support INotifyPropertyChange.
        public void UpdateUi()
        {
            kinectIndex.Text = _Kinect.InstanceIndex.ToString();
            kinectName.Text = _Kinect.InstanceName;
            kinectStatus.Text = _Kinect.Status.ToString();
        }
        #endregion Public API

        #region Init
        private void InitRuntime()
        {
            //Some Runtimes' status will be NotPowered, or some other error state. Only want to Initialize the runtime, if it is connected.
            if (_Kinect.Status == KinectStatus.Connected)
            {
                bool skeletalViewerAvailable = IsSkeletalViewerAvailable;

                // NOTE:  Skeletal tracking only works on one Kinect per process right now.
                RuntimeOptions = skeletalViewerAvailable ?
                                     RuntimeOptions.UseDepthAndPlayerIndex | RuntimeOptions.UseSkeletalTracking | RuntimeOptions.UseColor
                                     : RuntimeOptions.UseDepth | RuntimeOptions.UseColor;
                _Kinect.Initialize(RuntimeOptions);
                skeletonPanel.Visibility = skeletalViewerAvailable ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                if (RuntimeOptions.HasFlag(RuntimeOptions.UseSkeletalTracking))
                {
                    _Kinect.SkeletonEngine.TransformSmooth = true;
                }
            }

            string[] array = {"HipCenterX", "HipCenterY", "HipCenterZ", "SpineX", "SpineY", "SpineZ", "ShoulderCenterX", 
                                 "ShoulderCenterY", "ShoulderCenterZ", "HeadX", "HeadY", "HeadZ", "ShoulderLeftX", 
                             "ShoulderLeftY", "ShoulderLeftZ", "ElbowLeftX", "ElbowLeftY", "ElbowLeftZ", "WristLeftX", 
                             "WristLeftY", "WristLeftZ", "HandLeftX", "HandLeftY", "HandLeftZ", "ShoulderRightX", 
                             "ShoulderRightY", "ShoulderRightZ", "ElbowRightX", "ElbowRightY", "ElbowRightZ", 
                             "WristRightX", "WristRightY", "WristRightZ", "HandRightX", "HandRightY", "HandRightZ", 
                             "HipLeftX", "HipLeftY", "HipLeftZ", "KneeLeftX", "KneeLeftY", "KneeLeftZ", "AnkleLeftX", 
                             "AnkleLeftY", "AnkleLeftZ", "FootLeftX", "FootLeftY", "FootLeftZ", "HipRightX", "HipRightY", 
                             "HipRightZ", "KneeRightX", "KneeRightY", "KneeRightZ", "AnkleRightX", "AnkleRightY", 
                             "AnkleRightZ", "FootRightX", "FootRightY", "FootRightZ", "Angle", "x", "y", "z", "w"};

            for (int i = 0; i < 65; i++)
            {
                DataColumn column1 = new DataColumn(array[i], System.Type.GetType("System.Double"));
                dt1.Columns.Add(column1);
            }
        }
        
        /// <summary>
        /// Skeletal tracking only works on one Kinect right now.  So return false if it is already in use.
        /// </summary>
        private bool IsSkeletalViewerAvailable
        {
            get { return KinectNui.Runtime.Kinects.All(k => k.SkeletonEngine == null); }
        }

        #endregion Init

        #region Skeleton Processing
        private void nui_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonFrame skeletonFrame = e.SkeletonFrame;
            
            //KinectSDK TODO: this shouldn't be needed, but if power is removed from the Kinect, you may still get an event here, but skeletonFrame will be null.
            if (skeletonFrame == null)
            {
                return;
            }

            int iSkeleton = 0;
            
            Brush[] brushes = new Brush[6];
            brushes[0] = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            brushes[1] = new SolidColorBrush(Color.FromRgb(0, 255, 0));
            brushes[2] = new SolidColorBrush(Color.FromRgb(64, 255, 255));
            brushes[3] = new SolidColorBrush(Color.FromRgb(255, 255, 64));
            brushes[4] = new SolidColorBrush(Color.FromRgb(255, 64, 255));
            brushes[5] = new SolidColorBrush(Color.FromRgb(128, 128, 255));

            skeletonCanvas.Children.Clear();
            int trackedSkeleton = 1;
            foreach (SkeletonData data in skeletonFrame.Skeletons)
            {
                if (SkeletonTrackingState.Tracked == data.TrackingState)
                {
                    // Draw bones
                    Brush brush = brushes[iSkeleton % brushes.Length];
                    skeletonCanvas.Children.Add(getBodySegment(data.Joints, brush, JointID.HipCenter, JointID.Spine, JointID.ShoulderCenter, JointID.Head));
                    skeletonCanvas.Children.Add(getBodySegment(data.Joints, brush, JointID.ShoulderCenter, JointID.ShoulderLeft, JointID.ElbowLeft, JointID.WristLeft, JointID.HandLeft));
                    skeletonCanvas.Children.Add(getBodySegment(data.Joints, brush, JointID.ShoulderCenter, JointID.ShoulderRight, JointID.ElbowRight, JointID.WristRight, JointID.HandRight));
                    skeletonCanvas.Children.Add(getBodySegment(data.Joints, brush, JointID.HipCenter, JointID.HipLeft, JointID.KneeLeft, JointID.AnkleLeft, JointID.FootLeft));
                    skeletonCanvas.Children.Add(getBodySegment(data.Joints, brush, JointID.HipCenter, JointID.HipRight, JointID.KneeRight, JointID.AnkleRight, JointID.FootRight));

                    double[] array = new double[60];
                    int i = 0;

                    // Draw joints
                    foreach (Joint joint in data.Joints)
                    {
                        array[i] = joint.Position.X;
                        array[i+1] = joint.Position.Y;
                        array[i+2] = joint.Position.Z;
                        i = i + 3;
                        Point jointPos = getDisplayPosition(joint);
                        Line jointLine = new Line();
                        jointLine.X1 = jointPos.X - 3;
                        jointLine.X2 = jointLine.X1 + 6;
                        jointLine.Y1 = jointLine.Y2 = jointPos.Y;
                        jointLine.Stroke = jointColors[joint.ID];
                        jointLine.StrokeThickness = 6;
                        skeletonCanvas.Children.Add(jointLine);
                    }
                    if (trackedSkeleton == 1)
                    {
                        DataRow dr = dt1.NewRow();
                        dr["HipCenterX"] = array[0];
                        dr["HipCenterY"] = array[1];
                        dr["HipCenterZ"] = array[2];
                        dr["SpineX"] = array[3];
                        dr["SpineY"] = array[4];
                        dr["SpineZ"] = array[5];
                        dr["ShoulderCenterX"] = array[6];
                        dr["ShoulderCenterY"] = array[7];
                        dr["ShoulderCenterZ"] = array[8];
                        dr["HeadX"] = array[9];
                        dr["HeadY"] = array[10];
                        dr["HeadZ"] = array[11];
                        dr["ShoulderLeftX"] = array[12];
                        dr["ShoulderLeftY"] = array[13];
                        dr["ShoulderLeftZ"] = array[14];
                        dr["ElbowLeftX"] = array[15];
                        dr["ElbowLeftY"] = array[16];
                        dr["ElbowLeftZ"] = array[17];
                        dr["WristLeftX"] = array[18];
                        dr["WristLeftY"] = array[19];
                        dr["WristLeftZ"] = array[20];
                        dr["HandLeftX"] = array[21];
                        dr["HandLeftY"] = array[22];
                        dr["HandLeftZ"] = array[23];
                        dr["ShoulderRightX"] = array[24];
                        dr["ShoulderRightY"] = array[25];
                        dr["ShoulderRightZ"] = array[26];
                        dr["ElbowRightX"] = array[27];
                        dr["ElbowRightY"] = array[28];
                        dr["ElbowRightZ"] = array[29];
                        dr["WristRightX"] = array[30];
                        dr["WristRightY"] = array[31];
                        dr["WristRightZ"] = array[32];
                        dr["HandRightX"] = array[33];
                        dr["HandRightY"] = array[34];
                        dr["HandRightZ"] = array[35];
                        dr["HipLeftX"] = array[36];
                        dr["HipLeftY"] = array[37];
                        dr["HipLeftZ"] = array[38];
                        dr["KneeLeftX"] = array[39];
                        dr["KneeLeftY"] = array[40];
                        dr["KneeLeftZ"] = array[41];
                        dr["AnkleLeftX"] = array[42];
                        dr["AnkleLeftY"] = array[43];
                        dr["AnkleLeftZ"] = array[44];
                        dr["FootLeftX"] = array[45];
                        dr["FootLeftY"] = array[46];
                        dr["FootLeftZ"] = array[47];
                        dr["HipRightX"] = array[48];
                        dr["HipRightY"] = array[49];
                        dr["HipRightZ"] = array[50];
                        dr["KneeRightX"] = array[51];
                        dr["KneeRightY"] = array[52];
                        dr["KneeRightZ"] = array[53];
                        dr["AnkleRightX"] = array[54];
                        dr["AnkleRightY"] = array[55];
                        dr["AnkleRightZ"] = array[56];
                        dr["FootRightX"] = array[57];
                        dr["FootRightY"] = array[58];
                        dr["FootRightZ"] = array[59];
                        dr["Angle"] = _Kinect.NuiCamera.ElevationAngle;
                        dr["x"] = skeletonFrame.FloorClipPlane.X;
                        dr["y"] = skeletonFrame.FloorClipPlane.Y;
                        dr["z"] = skeletonFrame.FloorClipPlane.Z;
                        dr["w"] = skeletonFrame.FloorClipPlane.W;
                        dt1.Rows.Add(dr);
                    }
                    trackedSkeleton++;
                }
                iSkeleton++;
            } // for each skeleton
        }

        private Polyline getBodySegment(Microsoft.Research.Kinect.Nui.JointsCollection joints, Brush brush, params JointID[] ids)
        {
            PointCollection points = new PointCollection(ids.Length);
            for (int i = 0; i < ids.Length; ++i)
            {
                points.Add(getDisplayPosition(joints[ids[i]]));
            }

            Polyline polyline = new Polyline();
            polyline.Points = points;
            polyline.Stroke = brush;
            polyline.StrokeThickness = 5;
            return polyline;
        }

        private Point getDisplayPosition(Joint joint)
        {
            float depthX, depthY;
            Kinect.SkeletonEngine.SkeletonToDepthImage(joint.Position, out depthX, out depthY);
            depthX = depthX * 320; //convert to 320, 240 space
            depthY = depthY * 240; //convert to 320, 240 space
            int colorX, colorY;
            ImageViewArea iv = new ImageViewArea();
            // only ImageResolution.Resolution640x480 is supported at this point
            Kinect.NuiCamera.GetColorPixelCoordinatesFromDepthPixel(ImageResolution.Resolution640x480, iv, (int)depthX, (int)depthY, (short)0, out colorX, out colorY);

            // map back to skeleton.Width & skeleton.Height
            return new Point((int)(skeletonCanvas.Width * colorX / 640.0), (int)(skeletonCanvas.Height * colorY / 480));
        }

        private static Dictionary<JointID, Brush> jointColors = new Dictionary<JointID, Brush>() { 
            {JointID.HipCenter, new SolidColorBrush(Color.FromRgb(169, 176, 155))},
            {JointID.Spine, new SolidColorBrush(Color.FromRgb(169, 176, 155))},
            {JointID.ShoulderCenter, new SolidColorBrush(Color.FromRgb(168, 230, 29))},
            {JointID.Head, new SolidColorBrush(Color.FromRgb(200, 0,   0))},
            {JointID.ShoulderLeft, new SolidColorBrush(Color.FromRgb(79,  84,  33))},
            {JointID.ElbowLeft, new SolidColorBrush(Color.FromRgb(84,  33,  42))},
            {JointID.WristLeft, new SolidColorBrush(Color.FromRgb(255, 126, 0))},
            {JointID.HandLeft, new SolidColorBrush(Color.FromRgb(215,  86, 0))},
            {JointID.ShoulderRight, new SolidColorBrush(Color.FromRgb(33,  79,  84))},
            {JointID.ElbowRight, new SolidColorBrush(Color.FromRgb(33,  33,  84))},
            {JointID.WristRight, new SolidColorBrush(Color.FromRgb(77,  109, 243))},
            {JointID.HandRight, new SolidColorBrush(Color.FromRgb(37,   69, 243))},
            {JointID.HipLeft, new SolidColorBrush(Color.FromRgb(77,  109, 243))},
            {JointID.KneeLeft, new SolidColorBrush(Color.FromRgb(69,  33,  84))},
            {JointID.AnkleLeft, new SolidColorBrush(Color.FromRgb(229, 170, 122))},
            {JointID.FootLeft, new SolidColorBrush(Color.FromRgb(255, 126, 0))},
            {JointID.HipRight, new SolidColorBrush(Color.FromRgb(181, 165, 213))},
            {JointID.KneeRight, new SolidColorBrush(Color.FromRgb(71, 222,  76))},
            {JointID.AnkleRight, new SolidColorBrush(Color.FromRgb(245, 228, 156))},
            {JointID.FootRight, new SolidColorBrush(Color.FromRgb(77,  109, 243))}
        };
        #endregion Skeleton Processing

        #region Private State
        private KinectNui.Runtime _Kinect;
        #endregion Private State

        public static DataTable dt1 = new DataTable();
    }
}
