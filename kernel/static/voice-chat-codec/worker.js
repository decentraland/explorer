!function(t,e){for(var r in e)t[r]=e[r]}(this,function(t){var e={};function r(i){if(e[i])return e[i].exports;var a=e[i]={i:i,l:!1,exports:{}};return t[i].call(a.exports,a,a.exports,r),a.l=!0,a.exports}return r.m=t,r.c=e,r.d=function(t,e,i){r.o(t,e)||Object.defineProperty(t,e,{enumerable:!0,get:i})},r.r=function(t){"undefined"!=typeof Symbol&&Symbol.toStringTag&&Object.defineProperty(t,Symbol.toStringTag,{value:"Module"}),Object.defineProperty(t,"__esModule",{value:!0})},r.t=function(t,e){if(1&e&&(t=r(t)),8&e)return t;if(4&e&&"object"==typeof t&&t&&t.__esModule)return t;var i=Object.create(null);if(r.r(i),Object.defineProperty(i,"default",{enumerable:!0,value:t}),2&e&&"string"!=typeof t)for(var a in t)r.d(i,a,function(e){return t[e]}.bind(null,a));return i},r.n=function(t){var e=t&&t.__esModule?function(){return t.default}:function(){return t};return r.d(e,"a",e),e},r.o=function(t,e){return Object.prototype.hasOwnProperty.call(t,e)},r.p="",r(r.s=1)}([function(t,e,r){"use strict";var i,a;r.r(e),function(t){t.ENCODE="ENCODE",t.DECODE="DECODE",t.DESTROY_ENCODER="DESTROY_ENCODER",t.DESTROY_DECODER="DESTROY_ENCODER"}(i||(i={})),function(t){t.ENCODE="ENCODE_OUTPUT",t.DECODE="DECODE_OUTPUT"}(a||(a={}));class s{constructor(t,e,r){if(!t||!e||!r)throw new Error("Invalid settings specified for the resampler.");this.resampler=null,this.fromSampleRate=t,this.toSampleRate=e,this.channels=r||0,this.initialize()}initialize(){this.fromSampleRate==this.toSampleRate?(this.resampler=t=>t,this.ratioWeight=1):(this.fromSampleRate<this.toSampleRate?(this.linearInterpolation(),this.lastWeight=1):(this.multiTap(),this.tailExists=!1,this.lastWeight=0),this.initializeBuffers(),this.ratioWeight=this.fromSampleRate/this.toSampleRate)}bufferSlice(t){return this.outputBuffer.subarray(0,t)}initializeBuffers(){this.outputBufferSize=Math.ceil(this.inputBufferSize*this.toSampleRate/this.fromSampleRate/this.channels*1.0000004768371582)+this.channels+this.channels;try{this.outputBuffer=new Float32Array(this.outputBufferSize),this.lastOutput=new Float32Array(this.channels)}catch(t){this.outputBuffer=Float32Array.of(),this.lastOutput=Float32Array.of()}}linearInterpolation(){this.resampler=t=>{let e,r,i,a,s,o,n,l,u,f=t.length,h=this.channels;if(f%h!=0)throw new Error("Buffer was of incorrect sample length.");if(f<=0)return Float32Array.of();for(e=this.outputBufferSize,r=this.ratioWeight,i=this.lastWeight,a=0,s=0,o=0,n=0,l=this.outputBuffer;i<1;i+=r)for(s=i%1,a=1-s,this.lastWeight=i%1,u=0;u<this.channels;++u)l[n++]=this.lastOutput[u]*a+t[u]*s;for(i-=1,f-=h,o=Math.floor(i)*h;n<e&&o<f;){for(s=i%1,a=1-s,u=0;u<this.channels;++u)l[n++]=t[o+(u>0?u:0)]*a+t[o+(h+u)]*s;i+=r,o=Math.floor(i)*h}for(u=0;u<h;++u)this.lastOutput[u]=t[o++];return this.bufferSlice(n)}}multiTap(){this.resampler=t=>{let e,r,i,a,s,o,n,l,u,f,h,c=t.length,p=this.channels;if(c%p!=0)throw new Error("Buffer was of incorrect sample length.");if(c<=0)return Float32Array.of();for(e=this.outputBufferSize,r=[],i=this.ratioWeight,a=0,o=0,n=0,l=!this.tailExists,this.tailExists=!1,u=this.outputBuffer,f=0,h=0,s=0;s<p;++s)r[s]=0;do{if(l)for(a=i,s=0;s<p;++s)r[s]=0;else{for(a=this.lastWeight,s=0;s<p;++s)r[s]=this.lastOutput[s];l=!0}for(;a>0&&o<c;){if(n=1+o-h,!(a>=n)){for(s=0;s<p;++s)r[s]+=t[o+(s>0?s:0)]*a;h+=a,a=0;break}for(s=0;s<p;++s)r[s]+=t[o++]*n;h=o,a-=n}if(0!==a){for(this.lastWeight=a,s=0;s<p;++s)this.lastOutput[s]=r[s];this.tailExists=!0;break}for(s=0;s<p;++s)u[f++]=r[s]/i}while(o<c&&f<e);return this.bufferSlice(f)}}resample(t){return this.inputBufferSize=t.length,this.fromSampleRate==this.toSampleRate?this.ratioWeight=1:(this.fromSampleRate<this.toSampleRate?this.lastWeight=1:(this.tailExists=!1,this.lastWeight=0),this.initializeBuffers(),this.ratioWeight=this.fromSampleRate/this.toSampleRate),this.resampler(t)}}function o(t){return t.data.sampleRate?t.data.sampleRate:24e3}self.LIBOPUS_WASM_URL="libopus.wasm",importScripts("libopus.wasm.js");const n={},l={};function u(t,e,r,i){e.working=!0,setTimeout((function a(){e.lastWorkTime=Date.now();let s=r(e);s?(s=s instanceof Uint8Array?Uint8Array.from(s):Float32Array.from(s),postMessage(i(s,t),[s.buffer]),setTimeout(a,0)):e.working=!1}),0)}function f(t,e){var r;null===(r=t[e])||void 0===r||r.destroy(),delete t[e]}onmessage=function(t){if(t.data.topic===i.ENCODE){const e=o(t),r=n[t.data.streamId]=n[t.data.streamId]||{working:!1,encoder:new libopus.Encoder(1,e,24e3,20,!0),lastWorkTime:Date.now(),destroy:function(){this.encoder.destroy()}},i=function(t){return Int16Array.from(t,t=>{let e=Math.floor(32767*t);return e=Math.min(32767,e),e=Math.max(-32768,e),e})}(function(t,e,r){if(r&&r!==e){return new s(r,e,1).resample(t)}return t}(t.data.samples,t.data.sampleRate,t.data.inputSampleRate));r.encoder.input(i),r.working||u(t.data.streamId,r,t=>t.encoder.output(),(t,e)=>({topic:a.ENCODE,streamId:e,encoded:t}))}if(t.data.topic===i.DECODE){const e=o(t),r=l[t.data.streamId]=l[t.data.streamId]||{working:!1,decoder:new libopus.Decoder(1,e),lastWorkTime:Date.now(),destroy:function(){this.decoder.destroy()}};r.decoder.input(t.data.encoded),r.working||u(t.data.streamId,r,t=>t.decoder.output(),(t,e)=>{return{topic:a.DECODE,streamId:e,samples:(r=t,Float32Array.from(r,t=>{let e=t>=0?t/32767:t/32768;return Math.fround(e)}))};var r})}if(t.data.topic===i.DESTROY_DECODER){const{streamId:e}=t.data;f(l,e)}if(t.data.topic===i.DESTROY_ENCODER){const{streamId:e}=t.data;f(n,e)}}},function(t,e,r){"use strict";r.r(e);const i=r(0);var a;i&&i.__esModule&&i.default&&new i.default((a=self,{onConnect(t){a.addEventListener("message",()=>t(),{once:!0})},onError(t){a.addEventListener("error",e=>{e.error?t(e.error):e.message&&t(Object.assign(new Error(e.message),{colno:e.colno,error:e.error,filename:e.filename,lineno:e.lineno,message:e.message}))})},onMessage(t){a.addEventListener("message",e=>{t(e.data)})},sendMessage(t){a.postMessage(t)},close(){"terminate"in a?a.terminate():"close"in a&&a.close()}}))}]));