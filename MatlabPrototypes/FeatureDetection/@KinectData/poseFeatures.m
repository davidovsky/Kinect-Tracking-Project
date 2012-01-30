function features = poseFeatures(obj, frameNumber)
	features=[...
		obj.f_hipAngle(frameNumber),...
		obj.f_kneeAngle(frameNumber), ...
		obj.f_spineStability(frameNumber), ...
		obj.f_elbowAngle_L(frameNumber), ...
		obj.f_elbowAngle_R(frameNumber), ...
		obj.f_headtilt(frameNumber), ...
		obj.f_headpitch(frameNumber), ...
		obj.f_ankleLevel(frameNumber), ...
		obj.f_handLevel(frameNumber), ...
		obj.f_hipLevel(frameNumber), ...
		obj.f_shoulderLevel(frameNumber), ...
		0,0,0,0 ...
	];
end