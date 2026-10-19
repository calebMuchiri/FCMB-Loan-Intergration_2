(function () {
    var link = document.querySelector("link[rel*='icon']") || document.createElement('link');
    link.type = 'image/x-icon';
    link.rel = 'shortcut icon';
    link.href = 'http://localhost:64707/ui/images/brnet.gif';
    document.getElementsByTagName('head')[0].appendChild(link);
    document.body.style.backgroundImage = "url('')";   
    //document.getElementById("api_info").innerHTML = '<div class="info_title"><image src="http://localhost:64707/ui/images/brnet.png"/><br/></div> <div class="info_description markdown"><font color="#027821"><b>BRNET OMNI API</b></font></div>'
    //document.getElementById("logo").innerHTML = '<div class="logo__img"><image src="http://localhost:64707/ui/images/brnet.png"/><br/></div>'
    document.getElementById('explore').style.visibility = "hidden";
    colorLinks("#042dfa")
    

})();

function colorLinks(hex) {
    var links = document.getElementsByTagName("a");
    for (var i = 0; i < links.length; i++) {
        if (links[i].href) {
            links[i].style.color = hex;
        }
    }
}