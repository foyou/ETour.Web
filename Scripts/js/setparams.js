(function () {
    //分销传参
    if (sessionStorage) {
        
        function getQueryStringValue(name) {
            var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)", "i");
            var r = window.location.search.substr(1).match(reg) || window.location.hash.substr(1).match(reg);
            if (r != null) return unescape(r[2]); return null;
        }

        var outId = getQueryStringValue("OUID");
        var allianceid = getQueryStringValue("Allianceid");
        var sid = getQueryStringValue("SID");

        if (outId && allianceid && sid) {
            sessionStorage["SellParams"] =JSON.stringify({
                outId: outId,
                allianceid: allianceid,
                sid: sid
            });
        }
    }
    
})();