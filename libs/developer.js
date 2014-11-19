// 单次位置请求执行的函数             
function getLocation() {
    navigator.geolocation.getCurrentPosition(mapIt, locationError);
}
//定位成功时，执行的函数
function mapIt(position) {
    var lon = position.coords.longitude;
    var lat = position.coords.latitude;
    // 指示浏览器获取高精度的位置，默认为false
    enableHighAcuracy: true;
    var point = new BMap.Point("" + lon + "", "" + lat + "");
    var gc = new BMap.Geocoder();
    translateCallback = function (point) {
        var marker = new BMap.Marker(point);
        gc.getLocation(point, function (rs) {
            var addComp = rs.addressComponents;
            $("#headerview h1").append("-" + addComp.city);
            //var infoWindow = new BMap.InfoWindow(addComp.city);
            //map.openInfoWindow(infoWindow, point);
        });
    }
    BMap.Convertor.translate(point, 0, translateCallback);
}
// 定位失败时，执行的函数
function locationError(error) {
    switch (error.code) {
        case error.PERMISSION_DENIED:
            alert("User denied the request for Geolocation.");
            break;
        case error.POSITION_UNAVAILABLE:
            alert("Location information is unavailable.");
            break;
        case error.TIMEOUT:
            alert("The request to get user location timed out.");
            break;
        case error.UNKNOWN_ERROR:
            alert("An unknown error occurred.");
            break;
    }
}
