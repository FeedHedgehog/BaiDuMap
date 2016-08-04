    'use strict';

	//login
	function loginServer(){
		$.post("http://192.168.60.173:8080/cbs/login", 
		{ 
			j_username:"admin",
			j_password:"111111",
			remember_me:"on",
			j_type:"mobile"
		}, 
			function(data,status){ 
			alert("Login 数据: \n" + data + "\n状态: " + status); 
		}); 
	}
	
	//get data from server
	function getDataFromServer(){
		$.get("http://httpbin.org/",function(data,status){
			alert("数据: " + data + "\n状态: " + status);
		  });
	}
	
	
	
    //create  and initialize the baidu map.
    function initMap(){
	  //create the baidu map
      createMap();
	  //set map's events
      setMapEvent();
	  //add the controls to baidu map
      addMapControl();
	  
    }
	
	//set the center of the map for "六盘水市"
    function createMap(){ 
      map = new BMap.Map("allmap"); 
      map.centerAndZoom(new BMap.Point(104.072222,30.663484),15);
	  window.setTimeout(function(){   
		   map.panTo(new BMap.Point(104.870896,26.594743));   
	  }, 2000);
    }
	
	//set map's events
    function setMapEvent(){
      map.enableScrollWheelZoom();
      map.enableKeyboard();
      map.enableDragging();
      map.enableDoubleClickZoom()
	  map.addEventListener("click",function(e){
		  if(e.overlay){
			selectAction();
		}
		else{
			unselectAction();
		}
	  });
      //custom events
	  addZoomEndHandler(15);	  
    }
	
	
	
	//add ClickEvent for tartget,and after that,open the InfoWindow which passed on
    function addClickHandler(target,window){
      target.addEventListener("click",function(){
        target.openInfoWindow(window);
      });
    }
	
	//when map zoom to the certain level,it will invoke the addMapOverlay method
	function addZoomEndHandler(inputzoom){
		map.addEventListener("zoomend",function(){
			//get the current zoom of the map
			var currentzoom = map.getZoom();
			//alert("当前的zoom:"+currentzoom);
			if(currentzoom >= inputzoom){
				//alert("at zoom:"+currentzoom+"zoomended!");
				//add the overlays to baidu map
				//addMapOverlay();
				addCustomPolyline(true);
				
				//addMarkerInfoWindow("imgDemo",sContent,inputzoom);				
			}	
			else{
				addCustomPolyline(false);
			}			
		});
	}
	
	//画多边形
	function addCustomPolyline(isShow){
		var polyline = new BMap.Polyline([
		  new BMap.Point(104.853864,26.598749),
          new BMap.Point(104.848474,26.593128),
          new BMap.Point(104.861553,26.58854),
          new BMap.Point(104.864931,26.587829),
          new BMap.Point(104.868165,26.58686),
          new BMap.Point(104.869458,26.592223),
          new BMap.Point(104.86989,26.593322),
          new BMap.Point(104.860619,26.595841),
          new BMap.Point(104.86141,26.597844),
          new BMap.Point(104.858607,26.598943),
          new BMap.Point(104.856738,26.598038),
          new BMap.Point(104.855373,26.598167),
          new BMap.Point(104.853792,26.598878),
          new BMap.Point(104.853792,26.598878),
          new BMap.Point(104.853792,26.598878),
          new BMap.Point(104.853792,26.598878)
		], {strokeColor:"blue", strokeWeight:20, strokeOpacity:0.5});   
		if(isShow){
			var pt1 = new BMap.Point(104.85044,26.595524);			
			addPolylineLabel(pt1,"100m");
			
			var pt2 = new BMap.Point(104.850979,26.595904);			
			addPolylineLabel(pt2,"120m");
			
			map.clearOverlays();			
			map.addOverlay(polyline); 	
			polyline.addEventListener("click",clickPolylineHandler);
			polyline.show();
		}
		else{
			map.clearOverlays();			
			polyline.hide();
		}		
		//alert(isShow);
	}
	
	//点击折线的时候改变颜色和透明度
	function clickPolylineHandler(e){
		
	}
	
	//add label to Polyline
	function addPolylineLabel(point,content){		
		var opts = {
			  position : point,    // 指定文本标注所在的地理位置
			  offset   : new BMap.Size(0, 0)    //设置文本偏移量
		}
		var label = new BMap.Label(content, opts);  // 创建文本标注对象
			label.setStyle({
				 color : "red",
				 fontSize : "12px",
				 height : "20px",
				 lineHeight : "20px",
				 fontFamily:"微软雅黑"
		});
		//禁止覆盖物在map.clearOverlays方法中被清除
		label.disableMassClear();
		map.addOverlay(label); 

	}
	
	var sContent =
	"<h4 style='margin:0 0 5px 0;padding:0.2em 0'>六盘水</h4>" + 
	"<img style='float:right;margin:4px' id='imgDemo' src='http://app.baidu.com/map/images/tiananmen.jpg' width='139' height='104' title='天安门'/>" + 
	"<p style='margin:0;line-height:1.5;font-size:13px;text-indent:2em'>六盘水是...</p>" + 
	"</div>";
	
	var polylines;
		
	function addMarkerInfoWindow(imgid,content,zoom){	
		var pt = new BMap.Point(104.857483,26.597535);	
		//alert(content);
		var cyIcon = new BMap.Icon("image/est.png", new BMap.Size(50, 63), {
                  anchor: new BMap.Size(10, 30),
                 infoWindowAnchor: new BMap.Size(25, 0)
             });
		var marker = new BMap.Marker(pt, { icon: cyIcon });  //创建标注		
	    var infoWindow = new BMap.InfoWindow(content);  // 创建信息窗口对象
	    map.centerAndZoom(pt, zoom);
		map.clearOverlays();
	    map.addOverlay(marker);
	    marker.addEventListener("click", function(){          
	         this.openInfoWindow(infoWindow);
	         //图片加载完毕重绘infowindow
	         document.getElementById(imgid).onload=function(){
		          infoWindow.redraw();   //防止在网速较慢，图片未加载时，生成的信息框高度比图片的总高度小，导致图片部分被隐藏
	         }
	    });
	}
	
	//add the overlays to baidu map
    function addMapOverlay(){
      var markers = [
        {content:"六盘水项目，撒了多久啊了dhl",title:"六盘水项目部",imageOffset: {width:0,height:3},position:{lat:26.597521,lng:104.856164}}
      ];	  
      for(var index = 0; index < markers.length; index++ ){
        var point = new BMap.Point(markers[index].position.lng,markers[index].position.lat);
        var marker = new BMap.Marker(point,{icon:new BMap.Icon("http://api.map.baidu.com/lbsapi/createmap/images/icon.png",new BMap.Size(20,25),{
          imageOffset: new BMap.Size(markers[index].imageOffset.width,markers[index].imageOffset.height)
        })});
        var label = new BMap.Label(markers[index].title,{offset: new BMap.Size(25,5)});
        var opts = {
          width: 200,
          title: markers[index].title,
          enableMessage: false
        };
        var infoWindow = new BMap.InfoWindow(markers[index].content,opts);
        marker.setLabel(label);
        addClickHandler(marker,infoWindow);
        map.addOverlay(marker);
		marker.setAnimation(BMAP_ANIMATION_BOUNCE); //跳动的动画
      };
      var labels = [
        {position:{lng:104.875567,lat:26.601462},content:"这个是文字标注"}
      ];
      for(var index = 0; index < labels.length; index++){
        var opt = { position: new BMap.Point(labels[index].position.lng,labels[index].position.lat )};
        var label = new BMap.Label(labels[index].content,opt);
        map.addOverlay(label);
      };
      var plOpts = [
        {strokeColor:"#00f",strokeWeight:"6",strokeOpacity:"0.6"}
      ];
      var plPath = [
        [
          new BMap.Point(104.853864,26.598749),
          new BMap.Point(104.848474,26.593128),
          new BMap.Point(104.861553,26.58854),
          new BMap.Point(104.864931,26.587829),
          new BMap.Point(104.868165,26.58686),
          new BMap.Point(104.869458,26.592223),
          new BMap.Point(104.86989,26.593322),
          new BMap.Point(104.860619,26.595841),
          new BMap.Point(104.86141,26.597844),
          new BMap.Point(104.858607,26.598943),
          new BMap.Point(104.856738,26.598038),
          new BMap.Point(104.855373,26.598167),
          new BMap.Point(104.853792,26.598878),
          new BMap.Point(104.853792,26.598878),
          new BMap.Point(104.853792,26.598878),
          new BMap.Point(104.853792,26.598878)
        ],
      ];
      for(var index = 0; index < plOpts.length; index++){
        var polyline = new BMap.Polyline(plPath[index],plOpts[index]);
        map.addOverlay(polyline);
      }
    }
	
    //add the controls to baidu map
    function addMapControl(){
      var scaleControl = new BMap.ScaleControl({anchor:BMAP_ANCHOR_BOTTOM_LEFT});
      scaleControl.setUnit(BMAP_UNIT_IMPERIAL);
      map.addControl(scaleControl);
      var navControl = new BMap.NavigationControl({anchor:BMAP_ANCHOR_TOP_LEFT,type:BMAP_NAVIGATION_CONTROL_LARGE});
      map.addControl(navControl);
      var overviewControl = new BMap.OverviewMapControl({anchor:BMAP_ANCHOR_BOTTOM_RIGHT,isOpen:true});
      map.addControl(overviewControl);
    }
	
	
	
	
	// 改变覆盖物状态  
	function selectAction(){  
		polylines = [
			  new BMap.Polyline([
			  new BMap.Point(104.853864,26.598749),
			  new BMap.Point(104.848474,26.593128),
			  new BMap.Point(104.861553,26.58854),
			  new BMap.Point(104.864931,26.587829),
			  new BMap.Point(104.868165,26.58686),
			  new BMap.Point(104.869458,26.592223),
			  new BMap.Point(104.86989,26.593322),
			  new BMap.Point(104.860619,26.595841),
			  new BMap.Point(104.86141,26.597844),
			  new BMap.Point(104.858607,26.598943),
			  new BMap.Point(104.856738,26.598038),
			  new BMap.Point(104.855373,26.598167),
			  new BMap.Point(104.853792,26.598878),
			  new BMap.Point(104.853792,26.598878),
			  new BMap.Point(104.853792,26.598878),
			  new BMap.Point(104.853792,26.598878)
			], {strokeColor:"blue", strokeWeight:20, strokeOpacity:0.5})
		];
	
		//alert("polylines.length:"+polylines.length);
		for (var i=0; i<polylines.length; i++)
		{			
			var style = polylines[i].getStrokeStyle();
			if(style == "solid"){
				polylines[i].setStrokeColor("green");  
				polylines[i].setStrokeOpacity(1);
				map.clearOverlays();
				map.addOverlay(polylines[i]);
				//alert("solied,and opacity is"+polylines[i].getStrokeOpacity());
			}
			else if(style == "dashed"){
				//alert("dashed");
				polylines[i].setStrokeColor("yellow"); 
			}
		}
	}  

	function unselectAction(){	
	    if(polylines!=null || polylines.length>0){
			for (var i=0; i<polylines.length; i++)
			{
				var style = polylines[i].getStrokeStyle();
				if(style == "solid"){
					polylines[i].setStrokeColor("blue");  
					polylines[i].setStrokeOpacity(0.5);
					map.clearOverlays();
					map.addOverlay(polylines[i]);
					//alert("solied,and opacity is"+polylines[i].getStrokeOpacity());
				}
				else if(style == "dashed"){
					//alert("dashed");
					polylines[i].setStrokeColor("yellow"); 
				}
			}
		}		
	}
  
  
    //此函数是与webBrowser交互的
    //js到form窗体的
    function NavigateToU3D(message){
	    //alert("U3d");
	    window.external.GetNavigateToU3DMessage(message);
    }
  
    //form push data to here,and next is to analyze the data
    function PushDataToHtml(data){
	    alert(data);
	    return data;
    }  

	
	
	
	
	
	
	
	