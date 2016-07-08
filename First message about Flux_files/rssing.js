

function gtmos(u) {var g = "rss"; window.location ="http://www."+g+"ing.com"+u; }
function rgttfu_same(u){ window.location = (u.split("").reverse().join(""));}
function wob(u){ window.open(u, "_blank");}
function rgttfu(u){ window.open((u.split("").reverse().join("")), "_blank");}
function gttfu(u) { window.location =u; }
function imgZoom(img, v) { imgZoomInt(img,v, v); }
function trim(str) {return String(str).replace(/^\s+|\s+$/g,'');}
function is_emptystr(str) { return (trim(str)  == '') ;}
function trim_encode(str) {return encodeURIComponent(trim(str));}


function sdl(s,l,d,j,o) {
   var r='', r0='',p=0;
   for(var i=o;i<d+o;i++) {
      if(i>=d) {r0=r0+s.substr(p,1);} else if(i<l) {r=r+s.substr(p,1);}
      p=(p+j)%d;
   }
   return ((r0+r));
}

function imgZoomInt(img,x,y) {
   if ( !img.usr_width ) {
      img.usr_width = img.width;
      img.usr_height = img.height;
   }
   img.width = x*img.usr_width;
   img.height = y*img.usr_height;
}

function imgZoomReset(img) {
   imgZoomInt(img, 1, 1);
}

function imgZoomDyna(e, img, max_zf, max_zf1) {
   if( !max_zf1) max_zf1 = max_zf;
   var zf_x, curr_x, left_x, right_x, middle_x;
   var zf_y, curr_y, left_y, right_y, middle_y;
   if ( !img.usr_width ) {
      img.usr_width = img.width;
      img.usr_height = img.height;
   }

   left_x = findPositionX(img);
   left_y = findPositionY(img);

   right_x = left_x+img.usr_width;
   right_y = left_y+img.usr_height;

   middle_x = (left_x+right_x)/2
   middle_y = (left_y+right_y)/2

   curr_x = e.clientX;
   curr_y = e.clientY;

   if ( curr_x>right_x || curr_x<left_x || curr_y>right_y || curr_y<left_y) {
      imgZoomInt(img, 1, 1);
      return;
   }

   // normalize to unit square/circle
   var x = ((curr_x-middle_x)/(right_x-middle_x));
   var y = ((curr_y-middle_y)/(right_y-middle_y));

   var warp_factor;
   var scale_factor;

   if ( (x>y && x>-y) || (x<y && x<-y) ) {
     warp_factor = 1+y*y;
   } else {
     warp_factor = 1+x*x;
   }

   //this ranges from 0 on the center to 1 on the outside
   scale_factor = (x*x+y*y)/warp_factor;
   scale_factor = Math.sqrt(scale_factor);

   // now switch it around and speed up scaling on the center
   // 1 on the inside, 0 on the outside
   scale_factor = (1-scale_factor)*(1-scale_factor);
   //scale_factor = (1-scale_factor);
   var xsf = 1+(max_zf-1)*scale_factor;
   var ysf = 1+(max_zf1-1)*scale_factor;
   imgZoomInt(img, xsf, ysf);
}

function rate(img,kind,indx,rank,val) {
  img.src = '/inc/img/click.gif';
  sendreq(img,['req','rate','kind',kind,'indx',indx,'rank',rank,'val',val,'url',get_url()],[],ratings_end, [0]);
}

function rreq(req,val,target) {
  var w = window,
    d = document,
    e = d.documentElement,
    g = d.getElementsByTagName('body')[0],
    x = w.innerWidth || e.clientWidth || g.clientWidth,
    y = w.innerHeight|| e.clientHeight|| g.clientHeight;

  var xmlhttp;
  if (window.XMLHttpRequest) { // code for IE7+, Firefox, Chrome, Opera, Safari
    xmlhttp=new XMLHttpRequest();
  } else { // code for IE6, IE5 
     xmlhttp=new ActiveXObject("Microsoft.XMLHTTP"); }
  xmlhttp.onreadystatechange=function() { if (target && xmlhttp.readyState==4 && xmlhttp.status==200) {
      document.getElementById(target).innerHTML=xmlhttp.responseText; } }
  //xmlhttp.open("GET","/request.php?indx="+rss_indx+"&req="+req+"&val="+val+"&url="+rss_url+"&ref="+document.referrer,true);
  var reqs="/request.php?";
  if (typeof rss_indx != 'undefined')reqs=reqs+"indx="+rss_indx+"&";
  if (typeof rss_url != 'undefined')reqs=reqs+"url="+rss_url+"&";
  reqs=reqs+"req="+req+"&val="+val+"&dd="+x+"x"+y;
  //xmlhttp.open("GET","/request.php?indx="+rss_indx+"&req="+req+"&val="+val+"&url="+rss_url"&dd="+x+"x"+y,true);
  xmlhttp.open("GET",reqs,true);
  xmlhttp.send();
}

function rareq(req,vala,target) {
  var xmlhttp;
  if (window.XMLHttpRequest) { // code for IE7+, Firefox, Chrome, Opera, Safari
    xmlhttp=new XMLHttpRequest();
  } else { // code for IE6, IE5 
    xmlhttp=new ActiveXObject("Microsoft.XMLHTTP"); 
  }
  xmlhttp.onreadystatechange=function() {
    if (target && xmlhttp.readyState==4 && xmlhttp.status==200) {
      document.getElementById(target).innerHTML=xmlhttp.responseText;    
    } 
  }
  var url="/request.php?req="+req;
  for (var i=0;i<vala.length;i=i+2) {
    url=url+'&'+vala[i]+'='+vala[i+1];
  }
  xmlhttp.open("GET",url,true);
  xmlhttp.send();
}

function htmlset(toTarget,fromTarget) {
  if( fromTarget == '' ) {
    document.getElementById(toTarget).innerHTML='';
  } else {
    document.getElementById(toTarget).innerHTML=document.getElementById(fromTarget).innerHTML;
  }
}

function findPositionX(obj) {
    var left = 0; 
    if(obj.offsetParent) { 
        while(1) { 
          left += obj.offsetLeft; 
          if(!obj.offsetParent) 
            break; 
          obj = obj.offsetParent; 
        } 
    } else if(obj.x) { 
        left += obj.x; 
    } 
    return left; 
}

function findPositionY(obj) {
    var top = 0; 
    if(obj.offsetParent) { 
        while(1) { 
          top += obj.offsetTop; 
          if(!obj.offsetParent) 
            break; 
          obj = obj.offsetParent; 
        } 
    } else if(obj.y) { 
        top += obj.y; 
    } 
    return top; 
}

function setHttp(url) {
  url = trim(url);
  if (url.search(/^http[s]?\:\/\//) == 0) return url;
  return ('http://'+url);
}

function json2arr(json_str) {
   return eval("("+json_str+")");
}

function create_xmlhttprss() {
  var xmlhttp;
  if (window.XMLHttpRequest) {
    xmlhttp=new XMLHttpRequest(); // code for IE7+, Firefox, Chrome, Opera, Safari
  } else {
     xmlhttp=new ActiveXObject("Microsoft.XMLHTTP"); // code for IE6, IE5
  }
  return xmlhttp;
}

function arr2qry(a,s0,s1){
  var q='';
  if(a) for (var i=0;i<a.length/2;i++) {
     if ( a[2*i+1] instanceof Array) {
        var suba=a[2*i+1];
        for (var j=0;j<suba.length;j++) {
           q = q+((i+j==0)?s0:s1)+trim(a[2*i])+'[]='+trim_encode(suba[j]);
        }
     } else
     if(trim(a[2*i+1])!="") {
        q = q+((i==0)?s0:s1)+trim(a[2*i])+'='+trim_encode(a[2*i+1]);
     }
  }
  return q;
}

function sendreq(el,gargs,pargs,cb,cba) {
  var xmlhttp = create_xmlhttprss();
  if(cb) xmlhttp.onreadystatechange=function() { if (xmlhttp.readyState==4 && xmlhttp.status==200) {
       cb(el,xmlhttp.responseText,cba);
  } }
  var url = "/request.php"+arr2qry(gargs,'?','&');
  var msg = arr2qry(pargs,'','&');
  var rtype=(msg=='')?'GET':'POST';
  xmlhttp.open(rtype,url,true);
  xmlhttp.setRequestHeader("Content-type","application/x-www-form-urlencoded");
  xmlhttp.send(msg);
}

function testSameOrigin(url) {
  var loc = window.location;
  var a = document.createElement('a');
  a.href = url;
  return a.hostname == loc.hostname &&
         a.port == loc.port &&
         a.protocol == loc.protocol;
}

function get_dim() {
  var w = window,
    d = document,
    e = d.documentElement,
    g = d.getElementsByTagName('body')[0],
    width = w.innerWidth || e.clientWidth || g.clientWidth,
    height = w.innerHeight|| e.clientHeight|| g.clientHeight;
    var dim = width+"x"+height;
  return dim;
}

function rq2json(q) {
  var res = "";
  res = res + '[';
  for (var i=0;i<q.length;i++) {
    res = res + ((i==0)?'[':',[');
    for (var j=0;j<q[i].length;j++) {
      res = res + ((j==0)?'':',') + q[i][j];
    }
    res = res + ']';
  }
  res = res + ']';
  return res;
}

function gtfooms() {
  var qs  = document.referrer.toLowerCase();
  if (qs.indexOf("//keywords-monitoring-your-success.com/") !== -1) return 0;
  if (qs.indexOf("//keywords-monitoring-success.com/") !== -1) return 0;
  if (qs.indexOf("//free-video-tool.com/") !== -1) return 0;
  if (qs.indexOf("//rank-checker.online/") !== -1) return 0;
  if (qs.indexOf("//fix-website-errors.com/") !== -1) return 0;
  return 1;
}

function getratings(el, q) {
  var qs  = rq2json(q);
  //sendreq(el,['req','getratings','qs',qs,'url',get_url(),'dd',get_dim(),'rrr',document.referrer],[],ratings_end, [1]);
  sendreq(el,['req','getratings','qs',qs,'url',get_url(),'dd',get_dim()],[],ratings_end, [1]);
}

function ratings_end(el, rsp, cba) {
  var sr = json2arr(rsp);
  if (!sr.success) return;
  var srsp = sr.reply;

  var active=cba[0];

  srsp = sr.reply;
  //document.getElementById('ratingDiv').innerHTML=srsp;

  for (key in srsp) {
    rating_cfg(srsp[key], active);
  }
}

function img_cfg(id, src, indx, val, active) {
   var img = document.getElementById(id);
   img.src = src;
   if(active==1) {
     img.setAttribute("onmousemove","imgZoomDyna(event,this,1.6);");
     img.setAttribute("onmouseout","imgZoomReset(this);");
     img.setAttribute("onclick","rate(this,0,"+indx+",-1,"+val+");");
     img.style.cursor="pointer";
   } else {
     img.onmousemove="";
     img.onmouseout="";
     img.onclick="";
     img.style.cursor="";
   }
}

function star_img(pos, r) {
   var img= (r>(pos-1+0.75))?("star_full.png"):((r>(pos-1+0.25))?("star_half.png"):("star_empty.png"));
   return "/inc/img/"+img;
}

function updn_cfg(kind,indx,iindx,rate,cnt,active) {
  if( kind==1) {
    var dscid = 'upVoteDesc-'+indx+'-'+iindx;
    var imgid = 'upVoteImg-'+indx+'-'+iindx;
    var asrc = "/inc/img/tup-blue.png";
    var isrc = "/inc/img/tup-gray.png";
  } else 
  if( kind==2) {
    var dscid = 'dnVoteDesc-'+indx+'-'+iindx;
    var imgid = 'dnVoteImg-'+indx+'-'+iindx;
    var asrc = "/inc/img/tdown-blue.png";
    var isrc = "/inc/img/tdown-gray.png";
  } else return;

  var d = document.getElementById(dscid);
  d.innerHTML= cnt;
  var img = document.getElementById(imgid);
  if(active==1) {
    img.setAttribute("onmousemove","imgZoomDyna(event,this,1.6);");
    img.setAttribute("onmouseout","imgZoomReset(this);");
    img.setAttribute("onclick","rate(this,"+kind+","+indx+","+iindx+",1);");
    img.setAttribute("src",asrc);
    img.style.cursor="pointer";
  } else {
    imgZoomReset(img);
    img.onmousemove="";
    img.onmouseout="";
    img.onclick="";
    img.style.cursor="";
    img.setAttribute("src",isrc);
  }
}

function star_cfg(indx, rate,cnt,active) {
  var d = document.getElementById('ratingDesc');
  d.innerHTML= (cnt==0)? ("click to rate") : (rate.toFixed(1)+" stars on "+cnt+ " votes");
  img_cfg('chanrate1', star_img(1,rate), indx, 1, active);
  img_cfg('chanrate2', star_img(2,rate), indx, 2, active);
  img_cfg('chanrate3', star_img(3,rate), indx, 3, active);
  img_cfg('chanrate4', star_img(4,rate), indx, 4, active);
  img_cfg('chanrate5', star_img(5,rate), indx, 5, active);
}

function mature_cfg(indx, rate, cnt, active) {
  var d = document.getElementById('matureDesc');
  if( active == 1 ) {
     htmlset('matureBtn','mature1');
     d.innerHTML = ""+cnt+"";
  } else {
     htmlset('matureBtn','');
     d.innerHTML = ""+cnt+"";
  }
}

function rating_cfg(r, active) {
  //0:kind 1:indx, 2:item 3:cnt 4:cnt
  switch(r[0]) {
    case "0":
      star_cfg(r[1], r[4], r[3], active);
      break;
    case "1":
      updn_cfg(r[0],r[1],r[2],r[4],r[3],active);
      break;
    case "2":
      updn_cfg(r[0],r[1],r[2],r[4],r[3],active);
      break;
    case "3":
      mature_cfg(r[1], r[4], r[3], active);
      break;
  }
}

function verify_chan(form,dowarn) {
  var url = trim(form.chanurl.value);
  if( is_emptystr(url) ) {
    if(dowarn) alert("no url specified");
    return;
  }

  form.vchanindx.value = "";
  form.vchanurl.value = "";
  form.chantitle.value = "";
  form.chandesc.value = "";
  form.chancnt.value = "";
  document.getElementById('chanurlname').innerHTML="";
  document.getElementById('chanclaim').innerHTML="";
  //alert(form_entry.value);
  sendreq(null,['req','verifurl','kind',1,'url',url],[],verify_chan_end, [form]);
}

function verify_chan_end(el, rsp, cba) {
  var sr = json2arr(rsp);
  if (!sr.success) return;
  var form=cba[0];
  var srsp = sr.reply;
  srsp = sr.reply;
  //alert(srsp.error+'   '+srsp.durl);
  if( srsp.error != '' ) {
     alert(srsp.error);
     return;
  }
  form.vchanindx.value = srsp.indx;
  form.vchanurl.value = srsp.chanurl;
  form.chantitle.value = srsp.title;
  form.chandesc.value = srsp.desc;
  form.chancnt.value = srsp.cnt;
  document.getElementById('chanurlname').innerHTML=srsp.url;
  document.getElementById('chanclaim').innerHTML="Note: You may remove this channel from our site immediately by claiming it <a href='/account.php?a=mmc&r="+srsp.indx+"'>here</a>.";
}

function verify_item(form,dowarn) {
  var url = trim(form.itemurl.value);
  if( is_emptystr(url) ) {
    if (dowarn) alert("no url specified");
    return;
  }

  form.vchanindx.value = "";
  form.vitemindx.value = "";
  form.vitemurl.value = "";
  form.itemtitle.value = "";
  document.getElementById('itemdesc').innerHTML="";
  sendreq(null,['req','verifurl','kind',2,'url',url],[],verify_item_end, [form]);
}

function verify_item_end(el, rsp, cba) {
  var sr = json2arr(rsp);
  if (!sr.success) return;
  var form=cba[0];
  var srsp = sr.reply;
  srsp = sr.reply;
  if( srsp.error != '' ) {
     alert(srsp.error);
     return;
  }
  form.vchanindx.value = srsp.indx;
  form.vitemindx.value = srsp.itemindx;
  form.vitemurl.value = srsp.itemurl;
  form.itemtitle.value = srsp.title;
  var el = document.getElementById('itemdesc');
  el.innerHTML = srsp.desc;
  //el.style.display = 'none';
  //el.style.display = 'block';
  //var div = document.createElement('div');
  //div.innerHTML=srsp.desc;
  //div.setAttribute('type', 'text/html');
  //document.getElementById('itemdesc').appendChild(div);
  //document.getElementById('itemdesc').load(srsp.desc);
}

function fill_addthis(indx, rank) {
   var div_id = 'addthis-'+indx+'-'+rank;
   var divel = document.getElementById(div_id);
   divel.setAttribute('class', 'addthis_toolbox addthis_default_style addthis_32x32_style');
   divel.innerHTML =
      '<a class="addthis_button_compact"></a>'+
      '<a class="addthis_button_favorites"></a>'+
      '<a class="addthis_button_google"></a>'+
      '<a class="addthis_button_print"></a>'+
      '<a class="addthis_button_email"></a>'+
    //'<a class="addthis_button_mailto"></a>'+
      '<a class="addthis_button_gmail"></a>'+
      '<a class="addthis_button_facebook"></a>'+
      '<a class="addthis_button_twitter"></a>'+
      '<a class="addthis_button_linkedin"></a>'+
      '<a class="addthis_button_reddit"></a>'+
      '<a class="addthis_button_pinterest_share"></a>'+
      '';

   addthis.toolbox('#'+div_id);
   addthis.toolbox('#'+div_id);
}

function flipshare(indx, rank) {
  var menu_name='itemshare';
  fill_addthis(indx, rank);
  flipitemdisplay (indx, rank, menu_name);
}

function flipmenu(indx, rank) {
  var menu_name='itemmenu';
  flipitemdisplay (indx, rank, menu_name);
}

function flipitemdisplay (indx, rank, menu_name) {
  flipdisplaystyle(menu_name+'-'+indx+'-'+rank);
}

function flipdisplaystyle (elid, disp_mode) {
  if(!disp_mode) disp_mode='block';
  var el = document.getElementById(elid);
  if( el.style.display == 'none' ) {
     el.style.display = disp_mode;
  } else {
     el.style.display = 'none';
  }
}

function chkcheck(ffield, msg) {
  if( (ffield.checked) == false) {
    alert(msg);
    return 1;
 }
 return 0;
}

function chkfield(ffield, msg) {
  if( trim(ffield.value) == '') {
    alert(msg);
    return 1;
 }
 return 0;
}

function sendjmsg(msgarr) {
  document.getElementById("umsgbtn").innerHTML="sending.....";
  sendreq(null,['req','jmsg','flag',1],msgarr, sendmsg_end);
}

function sendmsg_end(el, rsp, cba) {
  umsgresp(rsp);
}

function sendmsg() {
  var uname = trim(document.cForm.uname.value);
  if ( uname == "" ) {umsgresp("please enter your name",1);return}
  var uemail = trim(document.cForm.uemail.value);
  if ( uemail == "" ) {umsgresp("please enter your email",1);return}
  var umsg = trim(document.cForm.umsg.value);
  if ( umsg == "" ) {umsgresp("Please complete your comment first.",1);return}
  var xmlhttp = create_xmlhttprss();
  xmlhttp.onreadystatechange=function() { if (xmlhttp.readyState==4 && xmlhttp.status==200) {
       umsgresp(xmlhttp.responseText);
  } }
  var msg ="name="+uname+" ; email="+uemail+" ; msg="+umsg;
  var f =document.cForm.flag.value;
  //msg = trim_encode(msg);
  xmlhttp.open("POST","/request.php?req=msg&f="+f+"&indx2="+msg,true);
  xmlhttp.setRequestHeader("Content-type","application/x-www-form-urlencoded");
  document.getElementById("umsgbtn").innerHTMLSave=document.getElementById("umsgbtn").innerHTML;
  document.getElementById("umsgbtn").innerHTML="sending.....";
  xmlhttp.send("e="+uemail+"&indx="+msg);
}

function umsgresp(resp,fail) {
   resp = resp.replace(/^\s+|\s+$/g, '') ; // trim it
   if (fail) {
      document.getElementById("umsgresp").innerHTML=resp;
   } else  {
      if( resp == "1" ) {
         document.getElementById("umsgresp").innerHTML="We are not interested in SEO services or any \"complete internet marketing package\". If you still want to send us this message, click the 'Send Message' button again.";
         document.cForm.flag.value = 1;
         document.getElementById("umsgbtn").innerHTML=document.getElementById("umsgbtn").innerHTMLSave;
      } else 
      if( resp == "2" ) {
         document.getElementById("umsgresp").innerHTML="Hi Stella. We really really REALLY honestly aren't interested in your complete Internet Package. If you still want to send us this message, click the 'Send Message' button again.";
         document.cForm.flag.value = 1;
         document.getElementById("umsgbtn").innerHTML=document.getElementById("umsgbtn").innerHTMLSave;
      } else {
         document.getElementById("umsgbtn").innerHTML="Sent.<p> *** Note: A copy of your message was sent to your email address. If you don't receive this message, then you have provided a wrong email address and we have no way of contacting you. <br> ***Note: In case our reply ends up in your spam mailbox, please check your spam folder for email from oneworldonesite" + "@" + "yahoo.com within the next few days.";
         document.getElementById("umsgresp").innerHTML="";
      }
   }
}
