function sig = IndFeat3(X,Y)
	[X2,X1,X0] = SplitData(X,Y);
	
	f_01=IndFeat([X0;X1], ...
		[zeros(size(X0,1),1);ones(size(X1,1),1)] ...
	);
	f_02=IndFeat([X0;X2], ...
		[zeros(size(X0,1),1);ones(size(X2,1),1)] ...
	);
	f_12=IndFeat([X1;X2], ...
		[zeros(size(X1,1),1);ones(size(X2,1),1)] ...
	);
	sig=[f_01;f_02;f_12];
end