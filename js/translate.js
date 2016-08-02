'use strict';



	//添加控件和比例尺
	function add_control(map){
		var top_left_control = new BMap.ScaleControl({anchor: BMAP_ANCHOR_TOP_LEFT});// 左上角，添加比例尺
		var top_left_navigation = new BMap.NavigationControl();  //左上角，添加默认缩放平移控件
		var top_right_navigation = new BMap.NavigationControl({anchor: BMAP_ANCHOR_TOP_RIGHT, type: BMAP_NAVIGATION_CONTROL_SMALL}); //右上角，仅包含平移和缩放按钮
	/*缩放控件type有四种类型:
	BMAP_NAVIGATION_CONTROL_SMALL：仅包含平移和缩放按钮；BMAP_NAVIGATION_CONTROL_PAN:仅包含平移按钮；BMAP_NAVIGATION_CONTROL_ZOOM：仅包含缩放按钮*/	
		
		map.removeControl(top_left_control);     
		map.removeControl(top_left_navigation);  
		map.removeControl(top_right_navigation); 
		
		map.addControl(top_left_control);        
		map.addControl(top_left_navigation);     
		// map.addControl(top_right_navigation);    
	}


	function getData(){	
		//定义SQL语句
		var sql = "select obj_id,obj_caption from mw_sys.mwt_om_obj where rownum<5000";
		//新建数据库连接对象和数据集存取对象
		var ConnDB = new ActiveXObject("ADODB.Connection");
		ConnDB.open("Provider=MSDAORA.1;Password=app;User ID=mw_app;Data Source=pms;Persist Security Info=True");
		var rs = new ActiveXObject("ADODB.Recordset");
		rs.ActiveConnection = ConnDB;
		rs.Open(sql);
		//遍历
		var s;
		var rownum=0;
		var shtml="<table cellpadding=0; cellspacing=0; style='border:solid 1px gray;'><thead><td style='border:solid 1px gray; text-align:center;'>行号</td><td style='border:solid 1px gray; text-align:center;'>作业文本ID号</td><td style='border:solid 1px gray;'>专业编号</td><tbody>";
		while(!rs.EOF)
		{
		  shtml+="<tr><td style='border:solid 1px gray; text-align:center;'>";
		  shtml+=rownum+1;
		  shtml+="</td>";
		  for(i = 0;i<rs.Fields.Count;++i)
		  {
			shtml+="<td style='border:solid 1px gray; background-color:yellow;'>";
			shtml+=rs.Fields(i).value==null?" ":rs.Fields(i).value;
			shtml+="</td>";
		  }
		  shtml+="</tr>";
		  rownum++;
		  if(rownum==1000)
		  break;
		  rs.MoveNext();
		}
		shtml+="</tbody></table>";
		rs.close();
		ConnDB.close();
		document.getElementById("data").innerHTML=shtml;
	  }
  
  
  //通过Point定位
  function LocateByPoint(point,level,map){    
				
	map.centerAndZoom(point, level);  // 初始化地图,设置中心点坐标和地图级别
	map.addControl(new BMap.MapTypeControl());   //添加地图类型控件
	map.addControl(new BMap.NavigationControl()); //添加地图缩放控件
	map.addControl(new BMap.ScaleControl()); //添加比例尺控件
	map.setCurrentCity("成都");          // 设置地图显示的城市 此项是必须设置的
	map.enableScrollWheelZoom(true);     //开启鼠标滚轮缩放
	
	var overView = new BMap.OverviewMapControl();
	var overViewOpen = new BMap.OverviewMapControl({isOpen:true, anchor: BMAP_ANCHOR_BOTTOM_RIGHT});
	map.addControl(overView);          //添加默认缩略地图控件
	map.addControl(overViewOpen);      //右下角，打开	
	DrawLines(map);
	var pt = BMap.Point(104.086016,30.659562);
	DrawIcon(point,14,14,map);
	//map.addEventListener("click", showInfo);		
	
	
	// var sContent =
	// "<h4 style='margin:0 0 5px 0;padding:0.2em 0'>六盘水</h4>" + 
	// "<img style='float:right;margin:4px' id='imgDemo' src='http://app.baidu.com/map/images/tiananmen.jpg' width='139' height='104' title='天安门'/>" + 
	// "<p style='margin:0;line-height:1.5;font-size:13px;text-indent:2em'>六盘水是...</p>" + 
	// "</div>";
	// var projectPoint = new BMap.Point(104.062875,30.665651);						
		// ShowProjectInfo("imgDemo",projectPoint,sContent,level);	
	
	map.addEventListener("tilesloaded",function(e){		
		//alert("地图加载完毕");
		//这里判断level的级别然后加上 逃生口图片
			
	});	
  }
  
  function showInfo(e){
		alert(e.point.lng + ", " + e.point.lat);		
  }
  
  //通过城市名称定位
  function LocateByName(name,level,map){
	var map = new BMap.Map("allmap");    // 创建Map实例	
	map.centerAndZoom("成都", level);  // 初始化地图,设置中心点坐标和地图级别	
	map.addControl(new BMap.MapTypeControl());   //添加地图类型控件
	map.setCurrentCity("成都");          // 设置地图显示的城市 此项是必须设置的
	map.enableScrollWheelZoom(true);     //开启鼠标滚轮缩放  
    	
  }
  
  // function addMarker(map,point,index){  // 创建图标对象   
  		
		// var myIcon = new BMap.Icon("js/image/est.png", new BMap.Size(23, 25), {    
		// // 指定定位位置。   
		// // 当标注显示在地图上时，其所指向的地理位置距离图标左上    
		// // 角各偏移10像素和25像素。您可以看到在本例中该位置即是   
		   // // 图标中央下端的尖角位置。    
		   // offset: new BMap.Size(10, 25),    
		   // // 设置图片偏移。   
		   // // 当您需要从一幅较大的图片中截取某部分作为标注图标时，您   
		   // // 需要指定大图的偏移位置，此做法与css sprites技术类似。    
		   // imageOffset: new BMap.Size(0, 0 - index * 25)   // 设置图片偏移    
		 // });      
		// // 创建标注对象并添加到地图   
		 // var marker = new BMap.Marker(point, {icon: myIcon});    
		 // //map.addOverlay(marker);    
		 // //alert("添加图片成功@");
		// }    
  
  //添加标记
  function addMarker(map,point){
	map.clearOverlays();
	var marker = new BMap.Marker(point);
	marker.addEventListener("click",attribute);
	map.addOverlay(marker);
  }
  
  
	function attribute(){
		var p = marker.getPosition();  //获取marker的位置
		//alert("marker的位置是："+p.lng+","+p.lat);
	}

  
  
  //画线
  function DrawLines(map){	
  
	map.clearOverlays();
	  //画线
	var pointA = new BMap.Point(103.959961,30.689485);  
	var pointAA = new BMap.Point(103.961165,30.691321);
	
	var pointB = new BMap.Point(103.960118,30.689702);  
	var pointBB = new BMap.Point(103.959943,30.690083);
			
	var polylineA = new BMap.Polyline([pointA,pointAA], {strokeColor:"blue", strokeWeight:16, strokeOpacity:1});  //定义折线
	var polylineB = new BMap.Polyline([pointB,pointBB], {strokeColor:"blue", strokeWeight:16, strokeOpacity:1});  //定义折线
	polylineA.addEventListener("click", showInfoA);
	polylineB.addEventListener("click", showInfoB);
	map.addOverlay(polylineA);     //添加折线到地图上
	map.addOverlay(polylineB);	
	//alert("画线完成！");
   }
  
  
  //注销点击事件
  function removeClick(map){
	  map.removeEventListener("click",showInfoA);
  }
  
  
  //点击事件
  function showInfoA(e){
	 alert('您点击了线条A');
  }	 
  function showInfoB(e){
	 alert('您点击了线条B');
  }
  
  //添加矢量图形五角星
  function DrawSymbol(point,scale,color,opacity,map){
	 //添加矢量图标
	 var vectorStar = new BMap.Marker(new BMap.Point(point.lng,point.lat), {
     // 初始化五角星symbol
     icon: new BMap.Symbol(BMap_Symbol_SHAPE_STAR, {
     scale: scale,
     fillColor: color,
     fillOpacity: 1
     })
  });
  map.addOverlay(vectorStar);
  }
  
  function DrawClockIcon(point,zoom,map){
	  var vectorCLOCK = new BMap.Marker(new BMap.Point(point.lng,point.lat), {
	  // 初始化闹钟形状的symbol
	  icon: new BMap.Symbol(BMap_Symbol_SHAPE_CLOCK, {
		scale: 12,
		strokeWeight: 10,
		fillColor: 'blue',
		fillOpacity: 10
	  })
	});
	map.addOverlay(vectorCLOCK);	
	map.setViewport({center:point,zoom:zoom})
	//alert("clock!");
  }
  
  //放一定层级后，添加逃生口图标
  function DrawIcon(newpoint,inputlevel,level,map){
	  if(level == inputlevel){
		  //map.clearOverlays(); 		  
		  // var lifeIcon = new BMap.Icon("http://app.baidu.com/map/images/tiananmen.jpg", new BMap.Size(300,157));  // 创建标注
		  // var pt1 = BMap.Point(104.072289,30.656393);
		  // var marker = new BMap.Marker(pt1,{icon:lifeIcon});
		  // map.addOverlay(marker);              // 将标注添加到地图中
		  // map.panTo(pt1); 
		  //alert("Icon added");
		  var pt = BMap.Point(104.062947,30.665526);
		  var myIcon = new BMap.Icon("http://developer.baidu.com/map/jsdemo/img/fox.gif", new BMap.Size(300,157));
		  var marker2 = new BMap.Marker(newpoint,{icon:myIcon});  // 创建标注
	      map.addOverlay(marker2);              // 将标注添加到地图中
	  }	  	 
  }
  
  //点击标签显示项目信息
  function ShowProjectInfo(imgid,point,sContent,level){
	  var marker = new BMap.Marker(point);
	  var infoWindow = new BMap.InfoWindow(sContent);  // 创建信息窗口对象
	  map.centerAndZoom(point, level);
	  map.addOverlay(marker);
	  marker.addEventListener("click", function(){          
	  this.openInfoWindow(infoWindow);
	  //图片加载完毕重绘infowindow
	  document.getElementById(imgid).onload = function (){
		  infoWindow.redraw();   //防止在网速较慢，图片未加载时，生成的信息框高度比图片的总高度小，导致图片部分被隐藏
	  }
	 });
	 alert("showProjectInfo");
  }
	
	
	
	
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  