function features = poseFeatures(obj, frameNumber)
	features=[...
		obj.f_hipAngle_L(frameNumber), ...
		obj.f_hipAngle_R(frameNumber), ...
		...
		obj.f_kneeAngle_L(frameNumber), ...
		obj.f_kneeAngle_R(frameNumber), ...
		...
		obj.f_spineStability(frameNumber), ...
		obj.f_spineCurl_pitch(frameNumber), ...
		obj.f_spineCurl_tilt(frameNumber), ...
		obj.f_spinePitch(frameNumber), ...
		obj.f_spineTilt(frameNumber), ...
		...
		obj.f_elbowAngle_L(frameNumber), ...
		obj.f_elbowAngle_R(frameNumber), ...
		...
		obj.f_headtilt(frameNumber), ...
		obj.f_headpitch(frameNumber), ...
		obj.f_headDepth(frameNumber), ...
		...
		obj.f_ankleLevel(frameNumber), ...
		obj.f_handLevel(frameNumber), ...
		obj.f_hipLevel(frameNumber), ...
		obj.f_shoulderLevel(frameNumber), ...
		obj.f_squatDepth(frameNumber)...
	];
end