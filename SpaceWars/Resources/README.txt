SpaceWars Client 2018 
Created by William Asimakis and Joshua Call 
Version 1.0 
CS 3500 

Design Notes and Reflection: 
	-The creation of the Networking class was straight forward enough thanks to the help in lectures and the outline guidelines on canvas. 
	-The creation and implementation of JSON deserialization was also easy to implement due to canvas outlines. 
	-The implementation and design of the SpaceController (our main control unit for the view and model) was the point of most contention. 
		The most difficutly was designing an efficient Space Controller and optimizing the correct methods and events to accurately paint 
		the correct picture within the DrawingAux. 
	-The drawing panel was straightforward enough however we did run into some optimization issues with building bitmaps per frame call. 
		It made more sense to include many prebuilt image pathways to negate the need to reupload images every frame call. 
	-The biggest issue we felt was a latency glitch that would cause freezing and frame lags.  This was not a compile-time or run-time error and was 
		due to poor time implementation of the invalidation of the form.  Thorough investigation proved that we were implementing an invalidation of the form 
		per correct message, instead of per frame, essentially doing more work than was needed to invalidate the form.  We instead called the invalidation after 
		and entire set of completed messages instead of in-line with each deserialization. 
Polish: 
	-The included of color-based health provides a nice visual aid when your ship is low.  The colors are divided into green(full or semi-full health)/yellow(middle health)
		/and red(low health).   
	-William Asimakis designed a simple sprite pattern for simple drawing animation of a ship explosion (due to sun or ship contact). We were able to implement the 
		sprite pattern by drawing over only cropped sections of the image determined by a DeathCounter variable within the Ship class.  This was MVC validated, as the Death Counter 
		could be a model for other use and does not directly tie into visualization. 
	-We added a simple backgroudn to the entire form. 
------------------------------------------------------------- 
Version 2.0 (PS8) 
[CURRENT ISSUES] 
	-Memory leak correalted to OverlappedData and Networking asynchronicity -> further evaluation paramount

[Design Notes and Reflection]: 
	-The core of the server was managing the differentiating race conditions exposed through various client interaction onto the general world structure. A unique interaction we 
	found was the design choice to multi-thread the different world updates (projectile, ship, and star update threads); however this actually resulted in a massive latency hit. 
