var gimgs =new Object;
var imgtags = ['sports','funny', 'sexy', 'beautiful', 'fashion', 'unique','nature','food', 'art'];


function tgreq(imgel,mode,tg) {
  var indx = imgel.getAttribute('indx');
  var rank = imgel.getAttribute('rank');
  var iurl = imgel.src;
  var tt   = '';
  if( tt=='' && imgel.getAttribute('title')) tt = imgel.getAttribute('title');
  if( tt=='' && imgel.getAttribute('alt')  ) tt = imgel.getAttribute('alt');
  if( tt=='' && imgel.getAttribute('atitle'))tt = imgel.getAttribute('atitle');
  tg=(tg)?(tg):'';

  //rreq('davoai', indx+","+rank+","+mode+","+iurl);
  rareq('davoai', ['indx',indx,'rank',rank,'mode',mode,'tg',tg,'iurl',iurl,'tt',tt]);
}

function tginv(imgel,tg) {
  var tid="tg::"+tg;
  if(imgel[tid]) {
    delete imgel[tid];
    tgreq(imgel,-2,tg);
    return 0;
  } else {
    imgel[tid] = 1;
    tgreq(imgel,-3,tg);
    return 1;
  }
}

function tgup(ael,val) {
  if(val==1) {
    ael.className = 'tagbtnon';
  } else {
    ael.className = 'tagbtnoff';
  }
}

function tgclk(ael,ud) {
  //if( el.className=="tagbtnlnk")return;
  var pel=ael;
  while (!pel.getAttribute('mnudivindx')) pel=pel.parentNode;
  var imgel = gimgs[pel.getAttribute('mnudivindx')];

 tgup(ael,tginv(imgel,ael.innerHTML));
}
function atag(v){
  v=v.replace(/^\s+|\s+$/g, '')
  v=v.replace(/\s+/g, ' ')
  v=v.toLowerCase();
  if(v!='' && imgtags.indexOf(v)<0) {
    imgtags.push(v);
  }
  return v;
}

function irreq(el, mode) {
  while (!el.getAttribute('mnudivindx')) el=el.parentNode;
  var imgel = gimgs[el.getAttribute('mnudivindx')];
  var levindx = parseInt(el.getAttribute('mnulevindx'));
  if(mode==null) {
    if(imgel.getAttribute('tplid')) {//1+open
       tplid=imgel.getAttribute('tplid');
    } else {//1stopen
       tgreq(imgel,-1);
       tplid='dav0';
    }
  } else
  if(mode==1) {//nomat
    tgreq(imgel,mode);
    tplid='dav1';
  } else
  if(mode==2) {//yesmat
    tgreq(imgel,mode);
    tplid='dav1';
  } else
  if(mode==3) {//markfav
    tgreq(imgel,mode);
    tplid='davm';
  } else
  if(mode==4) {//not used
    mode=mode;
  } else
  if(mode==5) {//addtagmnu
    tplid='davn';
  } else
  if(mode==6) {//addtag
    var divs =el.getElementsByTagName("input");
    if( divs.length==0) {
       var a="ahaha";
    }
    var newTag=atag(divs[0].value);
    tginv(imgel,newTag);
    tplid='dav1';
  } else
  if(mode==7) {//canceltag
    tplid='dav1';
  }
  imgel.setAttribute('tplid', tplid);

  if (levindx==0)
    showact(el, tplid);
}

function msg(str) {
  //return;
  var el = document.getElementById('msg');
  el.innerHTML=el.innerHTML+"<br>"+str;
}

function ael(el,c,i) {
  var a = document.createElement('a');
  a.href = 'javascript:;';
  a.innerHTML = i;
  a.onclick = function() {tgclk(a);};
  a.className = c;
  el.appendChild(a); 
  var s = document.createElement('span');
  s.innerHTML=" ";
  el.appendChild(s); 
}

function showact(mnuel, tmplid) {
  var tmpl = document.getElementById(tmplid).cloneNode(true);
  var divindx=mnuel.getAttribute('mnudivindx');
  //tmpl.style.width = '';
  //tmpl.style.height = '';
  tmpl.id = tmplid+"-"+divindx;
  //tmpl.style.visibility = "visible";

  if( mnuel.firstChild) 
     mnuel.replaceChild(tmpl, mnuel.firstChild);
  else 
     mnuel.appendChild(tmpl);

  if( tmplid=='dav1') {
     var imgel = gimgs[divindx];
     var divs =tmpl.getElementsByTagName("td");
     var tagc;
     for (var i=0;i<divs.length;i++) {
        if(divs[i].id=='tag_container'){tagc=divs[i];break;}
     }
     var s = document.createElement('span');
     for (var i=0;i<imgtags.length;i++) {
        var tid="tg::"+imgtags[i];
        var attr=imgel[tid];
        if( attr) {
           ael(s,'tagbtnon',imgtags[i]);
        } else {
           ael(s,'tagbtnoff',imgtags[i]);
        }
     }
     tagc.appendChild(s); 
  }
}

function enmenu(mnuel, divid, divindx, levindx) {
  mnuel.setAttribute('mnudivindx', divindx);
  mnuel.setAttribute('mnulevindx', levindx);
  mnuel.setAttribute('mnudivid', divid);
  irreq(mnuel);
}

function iid(el) {if(!el) return "NULL"; if(el.id) return el.id; if(el.localName) return el.localName; return "NULL1";}
function ipn(e) { if(e.myp) return e.myp; return e.parentNode; }

function mouseout_from_el_id_is(e, wrpel) {
  var fr_el = e.target || e.fromElement;
  var to_el_orig = e.relatedTarget || e.toElement;
  var to_el = to_el_orig;
  while (fr_el && fr_el != wrpel) { 
     if( ipn(fr_el) ) 
        fr_el = ipn(fr_el);
     else break;
  }
  while (to_el && to_el != wrpel && ipn(to_el)) to_el = ipn(to_el);
  var rres = ( to_el != wrpel) ? 1 : 0;
  return rres;
}

function doout_e(event, wrpel) {
  //return;
  event = (event) ? event : window.event; // for ie
  if (mouseout_from_el_id_is(event, wrpel)==0) return;
  doout(wrpel);
}

function doout(wrpel) {
  var currel = document.getElementById(wrpel.parentNode.currid);
  var imgel = wrpel.parentNode.origel;
  wrpel.parentNode.replaceChild(imgel, currel);

  if( wrpel.levindx == 0) {
     imgel.onmouseover=function() {
       endiv(this,wrpel.divindx,0); return false;
     }
  }
}

function endiv(imgel, divindx, levindx) {
  var nn = getNatural(imgel);
  if( nn.width < 100) return;
  if( nn.height < 100) return;
  if( levindx == 0 ) {
    var to = (gimgs[divindx].getAttribute('tplid'))? 000: 2000;
    var tid = setTimeout(function() {endiv_int(imgel, divindx, levindx);}, to);
    gimgs[divindx].setAttribute('tid', tid);
    imgel.onmouseout=function(event) {
      window.clearTimeout(gimgs[divindx].getAttribute('tid'));
      return false;
    }
  } else {
    endiv_int(imgel, divindx, levindx);
  }
}

function endiv_int(imgel, divindx, levindx) {
  var wrpel = document.createElement("div");
  var mnuel = document.createElement("div");
  var srcel = document.createElement("div");

  wrpel.id = get_divid("wrp",divindx,levindx);
  mnuel.id = get_divid("mnu",divindx,levindx);
  srcel.id = get_divid("src",divindx,levindx);

  var imgw = imgel.offsetWidth; 
  var imgh = imgel.offsetHeight;
  var imgwb = imgel.offsetWidth-imgel.width; 
  var imghb = imgel.offsetHeight-imgel.height;

  wrpel.appendChild(mnuel);
  wrpel.appendChild(srcel);
  imgel.parentNode.replaceChild(wrpel,imgel);

  wrpel.style.overflow="hidden";
  wrpel.style.border  ="thin solid blue";
  wrpel.style.display  ="inline-block";
  if( levindx > 0) {
     wrpel.style.margin="0 auto";
  }
  if( levindx >= 0) {
     wrpel.style.textAlign="center";
  }

  var emptwrpw = wrpel.offsetWidth;
  var emptwrph = wrpel.offsetHeight;

  var srcid = get_divid("src",divindx,levindx);
  enmenu(mnuel, srcid, divindx, levindx);

  var menuwrpw = wrpel.offsetWidth;
  var menuwrph = wrpel.offsetHeight;

  var clone = imgel.cloneNode(true);

  srcel.style.height=(imgh-menuwrph)+"px";
  srcel.style.width=(imgw-emptwrpw)+"px";

  wrpel.style.height=(imgh-emptwrph)+"px";
  wrpel.style.width=(imgw-emptwrpw)+"px";
  wrpel.style.margin=imgel.style.margin;
  wrpel.style.cssFloat = imgel.align;

  var srcelar = (imgh-menuwrph)/(imgw-emptwrpw);
  var imgelar = imgh/imgw;

  if( imgelar > srcelar) {
     // height is full
     imgel.style.height=(imgh-menuwrph-imghb)+"px";
     imgel.style.width=(imgh-menuwrph-imghb)/imgelar+"px";
  } else {
     imgel.style.width=(imgw-emptwrpw-imgwb)+"px";
     imgel.style.height=(imgw-emptwrpw-imgwb)*imgelar+"px";
  }
     
  srcel.appendChild(imgel);
  wrpel.parentNode.currid = wrpel.id;
  wrpel.parentNode.origel = clone;

  srcel.origel = imgel.cloneNode(true);

  mnuel.myp = mnuel.parentNode;
  srcel.myp = srcel.parentNode;
  wrpel.myp = wrpel.parentNode;
  imgel.myp = imgel.parentNode;

  wrpel.divindx=divindx;
  wrpel.levindx=levindx;

  imgel.onmouseover = null;
  imgel.onmouseout = null;
  wrpel.onmouseout=function(event) {
    doout_e(event, this);
    return false;
  }
  return wrpel;
}
  
function get_divid(type, divindx, levindx) { return type+'_'+divindx+'_'+levindx; }
function get_divel(type, divindx, levindx) {
   return document.getElementById(get_divid(type, divindx, levindx));
}

function getNatural (DOMelement) {
    var img = new Image();
    img.src = DOMelement.src;
    return {width: img.width, height: img.height};
}

function endivid(divid, divindx, levindx) {
  var divel=document.getElementById(divid);
  var imgs =divel.getElementsByTagName("img");
  var img =imgs[0];
  if(img.parentNode.parentNode.id != divel.parentNode.id) {
     doout(divel.children[0]);
     var imgs =divel.getElementsByTagName("img");
     var img =imgs[0];
  }
  endiv(img, divindx, levindx);
}

function instrument_imgs(top_div_id) {
  var topel = document.getElementById(top_div_id);
  var imgs = topel.getElementsByTagName("img");
  for (var i=0;i<imgs.length;i++) {
    gimgs[i] = imgs[i].cloneNode(true);
    //imgs[i].outerHTML="<div>"+imgs[i].outerHTML+"</div>";
    (function(divindx){
      imgs[i].onmouseover=function() {
        endiv(this,divindx,0); return false;
      };
    })(i);
  };
}
