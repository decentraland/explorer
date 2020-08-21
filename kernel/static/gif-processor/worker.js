!function(e,t){for(var r in t)e[r]=t[r]}(this,function(e){var t={};function r(n){if(t[n])return t[n].exports;var i=t[n]={i:n,l:!1,exports:{}};return e[n].call(i.exports,i,i.exports,r),i.l=!0,i.exports}return r.m=e,r.c=t,r.d=function(e,t,n){r.o(e,t)||Object.defineProperty(e,t,{enumerable:!0,get:n})},r.r=function(e){"undefined"!=typeof Symbol&&Symbol.toStringTag&&Object.defineProperty(e,Symbol.toStringTag,{value:"Module"}),Object.defineProperty(e,"__esModule",{value:!0})},r.t=function(e,t){if(1&t&&(e=r(e)),8&t)return e;if(4&t&&"object"==typeof e&&e&&e.__esModule)return e;var n=Object.create(null);if(r.r(n),Object.defineProperty(n,"default",{enumerable:!0,value:e}),2&t&&"string"!=typeof e)for(var i in e)r.d(n,i,function(t){return e[t]}.bind(null,i));return n},r.n=function(e){var t=e&&e.__esModule?function(){return e.default}:function(){return e};return r.d(t,"a",t),t},r.o=function(e,t){return Object.prototype.hasOwnProperty.call(e,t)},r.p="",r(r.s=6)}([function(e,t,r){"use strict";Object.defineProperty(t,"__esModule",{value:!0}),t.decompressFrames=t.decompressFrame=t.parseGIF=void 0;var n,i=(n=r(3))&&n.__esModule?n:{default:n},a=r(1),o=r(2),s=r(4),d=r(5);t.parseGIF=function(e){var t=new Uint8Array(e);return(0,a.parse)((0,o.buildStream)(t),i.default)};var c=function(e,t,r){if(e.image){var n=e.image,i=n.descriptor.width*n.descriptor.height,a=(0,d.lzw)(n.data.minCodeSize,n.data.blocks,i);n.descriptor.lct.interlaced&&(a=(0,s.deinterlace)(a,n.descriptor.width));var o={pixels:a,dims:{top:e.image.descriptor.top,left:e.image.descriptor.left,width:e.image.descriptor.width,height:e.image.descriptor.height}};return n.descriptor.lct&&n.descriptor.lct.exists?o.colorTable=n.lct:o.colorTable=t,e.gce&&(o.delay=10*(e.gce.delay||10),o.disposalType=e.gce.extras.disposal,e.gce.extras.transparentColorGiven&&(o.transparentIndex=e.gce.transparentColorIndex)),r&&(o.patch=function(e){for(var t=e.pixels.length,r=new Uint8ClampedArray(4*t),n=0;n<t;n++){var i=4*n,a=e.pixels[n],o=e.colorTable[a];r[i]=o[0],r[i+1]=o[1],r[i+2]=o[2],r[i+3]=a!==e.transparentIndex?255:0}return r}(o)),o}console.warn("gif frame does not have associated image.")};t.decompressFrame=c;t.decompressFrames=function(e,t){return e.frames.filter((function(e){return e.image})).map((function(r){return c(r,e.gct,t)}))}},function(e,t,r){"use strict";Object.defineProperty(t,"__esModule",{value:!0}),t.loop=t.conditional=t.parse=void 0;t.parse=function e(t,r){var n=arguments.length>2&&void 0!==arguments[2]?arguments[2]:{},i=arguments.length>3&&void 0!==arguments[3]?arguments[3]:n;if(Array.isArray(r))r.forEach((function(r){return e(t,r,n,i)}));else if("function"==typeof r)r(t,n,i,e);else{var a=Object.keys(r)[0];Array.isArray(r[a])?(i[a]={},e(t,r[a],n,i[a])):i[a]=r[a](t,n,i,e)}return n};t.conditional=function(e,t){return function(r,n,i,a){t(r,n,i)&&a(r,e,n,i)}};t.loop=function(e,t){return function(r,n,i,a){for(var o=[];t(r,n,i);){var s={};a(r,e,n,s),o.push(s)}return o}}},function(e,t,r){"use strict";Object.defineProperty(t,"__esModule",{value:!0}),t.readBits=t.readArray=t.readUnsigned=t.readString=t.peekBytes=t.readBytes=t.peekByte=t.readByte=t.buildStream=void 0;t.buildStream=function(e){return{data:e,pos:0}};var n=function(){return function(e){return e.data[e.pos++]}};t.readByte=n;t.peekByte=function(){var e=arguments.length>0&&void 0!==arguments[0]?arguments[0]:0;return function(t){return t.data[t.pos+e]}};var i=function(e){return function(t){return t.data.subarray(t.pos,t.pos+=e)}};t.readBytes=i;t.peekBytes=function(e){return function(t){return t.data.subarray(t.pos,t.pos+e)}};t.readString=function(e){return function(t){return Array.from(i(e)(t)).map((function(e){return String.fromCharCode(e)})).join("")}};t.readUnsigned=function(e){return function(t){var r=i(2)(t);return e?(r[1]<<8)+r[0]:(r[0]<<8)+r[1]}};t.readArray=function(e,t){return function(r,n,a){for(var o="function"==typeof t?t(r,n,a):t,s=i(e),d=new Array(o),c=0;c<o;c++)d[c]=s(r);return d}};t.readBits=function(e){return function(t){for(var r=function(e){return e.data[e.pos++]}(t),n=new Array(8),i=0;i<8;i++)n[7-i]=!!(r&1<<i);return Object.keys(e).reduce((function(t,r){var i=e[r];return i.length?t[r]=function(e,t,r){for(var n=0,i=0;i<r;i++)n+=e[t+i]&&Math.pow(2,r-i-1);return n}(n,i.index,i.length):t[r]=n[i.index],t}),{})}}},function(e,t,r){"use strict";Object.defineProperty(t,"__esModule",{value:!0}),t.default=void 0;var n=r(1),i=r(2),a={blocks:function(e){for(var t=[],r=0,n=(0,i.readByte)()(e);0!==n;n=(0,i.readByte)()(e))t.push((0,i.readBytes)(n)(e)),r+=n;for(var a=new Uint8Array(r),o=0,s=0;s<t.length;s++)a.set(t[s],o),o+=t[s].length;return a}},o=(0,n.conditional)({gce:[{codes:(0,i.readBytes)(2)},{byteSize:(0,i.readByte)()},{extras:(0,i.readBits)({future:{index:0,length:3},disposal:{index:3,length:3},userInput:{index:6},transparentColorGiven:{index:7}})},{delay:(0,i.readUnsigned)(!0)},{transparentColorIndex:(0,i.readByte)()},{terminator:(0,i.readByte)()}]},(function(e){var t=(0,i.peekBytes)(2)(e);return 33===t[0]&&249===t[1]})),s=(0,n.conditional)({image:[{code:(0,i.readByte)()},{descriptor:[{left:(0,i.readUnsigned)(!0)},{top:(0,i.readUnsigned)(!0)},{width:(0,i.readUnsigned)(!0)},{height:(0,i.readUnsigned)(!0)},{lct:(0,i.readBits)({exists:{index:0},interlaced:{index:1},sort:{index:2},future:{index:3,length:2},size:{index:5,length:3}})}]},(0,n.conditional)({lct:(0,i.readArray)(3,(function(e,t,r){return Math.pow(2,r.descriptor.lct.size+1)}))},(function(e,t,r){return r.descriptor.lct.exists})),{data:[{minCodeSize:(0,i.readByte)()},a]}]},(function(e){return 44===(0,i.peekByte)()(e)})),d=(0,n.conditional)({text:[{codes:(0,i.readBytes)(2)},{blockSize:(0,i.readByte)()},{preData:function(e,t,r){return(0,i.readBytes)(r.text.blockSize)(e)}},a]},(function(e){var t=(0,i.peekBytes)(2)(e);return 33===t[0]&&1===t[1]})),c=(0,n.conditional)({application:[{codes:(0,i.readBytes)(2)},{blockSize:(0,i.readByte)()},{id:function(e,t,r){return(0,i.readString)(r.blockSize)(e)}},a]},(function(e){var t=(0,i.peekBytes)(2)(e);return 33===t[0]&&255===t[1]})),u=(0,n.conditional)({comment:[{codes:(0,i.readBytes)(2)},a]},(function(e){var t=(0,i.peekBytes)(2)(e);return 33===t[0]&&254===t[1]})),l=[{header:[{signature:(0,i.readString)(3)},{version:(0,i.readString)(3)}]},{lsd:[{width:(0,i.readUnsigned)(!0)},{height:(0,i.readUnsigned)(!0)},{gct:(0,i.readBits)({exists:{index:0},resolution:{index:1,length:3},sort:{index:4},size:{index:5,length:3}})},{backgroundColorIndex:(0,i.readByte)()},{pixelAspectRatio:(0,i.readByte)()}]},(0,n.conditional)({gct:(0,i.readArray)(3,(function(e,t){return Math.pow(2,t.lsd.gct.size+1)}))},(function(e,t){return t.lsd.gct.exists})),{frames:(0,n.loop)([o,c,u,s,d],(function(e){var t=(0,i.peekByte)()(e);return 33===t||44===t}))}];t.default=l},function(e,t,r){"use strict";Object.defineProperty(t,"__esModule",{value:!0}),t.deinterlace=void 0;t.deinterlace=function(e,t){for(var r=new Array(e.length),n=e.length/t,i=function(n,i){var a=e.slice(i*t,(i+1)*t);r.splice.apply(r,[n*t,t].concat(a))},a=[0,4,2,1],o=[8,8,4,2],s=0,d=0;d<4;d++)for(var c=a[d];c<n;c+=o[d])i(c,s),s++;return r}},function(e,t,r){"use strict";Object.defineProperty(t,"__esModule",{value:!0}),t.lzw=void 0;t.lzw=function(e,t,r){var n,i,a,o,s,d,c,u,l,f,p,g,h,y,v,m,w=r,b=new Array(r),x=new Array(4096),B=new Array(4096),A=new Array(4097);for(s=(i=1<<(f=e))+1,n=i+2,c=-1,a=(1<<(o=f+1))-1,u=0;u<i;u++)x[u]=0,B[u]=u;for(p=g=h=y=v=m=0,l=0;l<w;){if(0===y){if(g<o){p+=t[m]<<g,g+=8,m++;continue}if(u=p&a,p>>=o,g-=o,u>n||u==s)break;if(u==i){a=(1<<(o=f+1))-1,n=i+2,c=-1;continue}if(-1==c){A[y++]=B[u],c=u,h=u;continue}for(d=u,u==n&&(A[y++]=h,u=c);u>i;)A[y++]=B[u],u=x[u];h=255&B[u],A[y++]=h,n<4096&&(x[n]=c,B[n]=h,0==(++n&a)&&n<4096&&(o++,a+=n)),c=d}y--,b[v++]=A[y],l++}for(l=v;l<w;l++)b[l]=0;return b}},function(e,t,r){"use strict";r.r(t);const n=r(7);var i;n&&n.__esModule&&n.default&&new n.default((i=self,{onConnect(e){i.addEventListener("message",()=>e(),{once:!0})},onError(e){i.addEventListener("error",t=>{t.error?e(t.error):t.message&&e(Object.assign(new Error(t.message),{colno:t.colno,error:t.error,filename:t.filename,lineno:t.lineno,message:t.message}))})},onMessage(e){i.addEventListener("message",t=>{e(t.data)})},sendMessage(e){i.postMessage(e)},close(){"terminate"in i?i.terminate():"close"in i&&i.close()}}))},function(e,t,r){"use strict";r.r(t);var n,i=(n="",{error(e,...t){"object"==typeof e&&e.stack?console.error(n+e,...t,e.stack):console.error(n+e,...t)},log(e,...t){t&&t[0]&&t[0].startsWith&&t[0].startsWith("The entity is already in the engine.")||console.log(n+e,...t)},warn(e,...t){console.log(n+e,...t)},info(e,...t){console.info(n+e,...t)},trace(e,...t){console.trace(n+e,...t)}}),a=r(0);const o=new OffscreenCanvas(1,1),s=o.getContext("2d"),d=new OffscreenCanvas(1,1),c=d.getContext("2d"),u=new OffscreenCanvas(1,1),l=o.getContext("2d");let f=void 0;{let e=new Array;async function p(e){const t=fetch(e.data.src),r=await t,n=await r.arrayBuffer(),i=await Object(a.parseGIF)(n),s=Object(a.decompressFrames)(i,!0),d=new Array,c=new Array;let l=void 0,p=void 0,h=!1;if(f=void 0,o.width=s[0].dims.width,o.height=s[0].dims.height,h=o.width>512||o.height>512,h){let e=o.width>o.height?o.width/512:o.height/512;u.width=o.width/e,u.height=o.height/e}for(const e in s){d.push(s[e].delay);const t=g(s[e],h);c.push(t.data.buffer),l&&p||(l=t.width,p=t.height)}self.postMessage({arrayBufferFrames:c,width:l,height:p,delays:d,sceneId:e.data.sceneId,componentId:e.data.componentId},c)}function g(e,t){f&&e.dims.width===f.width&&e.dims.height===f.height||(d.width=e.dims.width,d.height=e.dims.height,f=null==c?void 0:c.createImageData(e.dims.width,e.dims.height)),f&&(f.data.set(e.patch),null==c||c.putImageData(f,0,0),null==s||s.scale(1,-1),null==s||s.drawImage(d,e.dims.left,-(o.height-e.dims.top)));let r=null==s?void 0:s.getImageData(0,0,o.width,o.height);return null==s||s.setTransform(1,0,0,1,0,0),r&&t&&(null==l||l.drawImage(o,0,0,o.width,o.height,0,0,u.width,u.height),r=null==l?void 0:l.getImageData(0,0,u.width,u.height)),r}self.onmessage=t=>{!function(t){if(e.push(t),1===e.length){(async function(){for(;e.length>0;)await p(e[0]),e.splice(0,1)})().catch(e=>i.log(e))}}(t)}}}]));