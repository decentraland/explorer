(function(e, a) { for(var i in a) e[i] = a[i]; }(this, /******/ (function(modules) { // webpackBootstrap
/******/ 	// The module cache
/******/ 	var installedModules = {};
/******/
/******/ 	// The require function
/******/ 	function __webpack_require__(moduleId) {
/******/
/******/ 		// Check if module is in cache
/******/ 		if(installedModules[moduleId]) {
/******/ 			return installedModules[moduleId].exports;
/******/ 		}
/******/ 		// Create a new module (and put it into the cache)
/******/ 		var module = installedModules[moduleId] = {
/******/ 			i: moduleId,
/******/ 			l: false,
/******/ 			exports: {}
/******/ 		};
/******/
/******/ 		// Execute the module function
/******/ 		modules[moduleId].call(module.exports, module, module.exports, __webpack_require__);
/******/
/******/ 		// Flag the module as loaded
/******/ 		module.l = true;
/******/
/******/ 		// Return the exports of the module
/******/ 		return module.exports;
/******/ 	}
/******/
/******/
/******/ 	// expose the modules object (__webpack_modules__)
/******/ 	__webpack_require__.m = modules;
/******/
/******/ 	// expose the module cache
/******/ 	__webpack_require__.c = installedModules;
/******/
/******/ 	// define getter function for harmony exports
/******/ 	__webpack_require__.d = function(exports, name, getter) {
/******/ 		if(!__webpack_require__.o(exports, name)) {
/******/ 			Object.defineProperty(exports, name, { enumerable: true, get: getter });
/******/ 		}
/******/ 	};
/******/
/******/ 	// define __esModule on exports
/******/ 	__webpack_require__.r = function(exports) {
/******/ 		if(typeof Symbol !== 'undefined' && Symbol.toStringTag) {
/******/ 			Object.defineProperty(exports, Symbol.toStringTag, { value: 'Module' });
/******/ 		}
/******/ 		Object.defineProperty(exports, '__esModule', { value: true });
/******/ 	};
/******/
/******/ 	// create a fake namespace object
/******/ 	// mode & 1: value is a module id, require it
/******/ 	// mode & 2: merge all properties of value into the ns
/******/ 	// mode & 4: return value when already ns object
/******/ 	// mode & 8|1: behave like require
/******/ 	__webpack_require__.t = function(value, mode) {
/******/ 		if(mode & 1) value = __webpack_require__(value);
/******/ 		if(mode & 8) return value;
/******/ 		if((mode & 4) && typeof value === 'object' && value && value.__esModule) return value;
/******/ 		var ns = Object.create(null);
/******/ 		__webpack_require__.r(ns);
/******/ 		Object.defineProperty(ns, 'default', { enumerable: true, value: value });
/******/ 		if(mode & 2 && typeof value != 'string') for(var key in value) __webpack_require__.d(ns, key, function(key) { return value[key]; }.bind(null, key));
/******/ 		return ns;
/******/ 	};
/******/
/******/ 	// getDefaultExport function for compatibility with non-harmony modules
/******/ 	__webpack_require__.n = function(module) {
/******/ 		var getter = module && module.__esModule ?
/******/ 			function getDefault() { return module['default']; } :
/******/ 			function getModuleExports() { return module; };
/******/ 		__webpack_require__.d(getter, 'a', getter);
/******/ 		return getter;
/******/ 	};
/******/
/******/ 	// Object.prototype.hasOwnProperty.call
/******/ 	__webpack_require__.o = function(object, property) { return Object.prototype.hasOwnProperty.call(object, property); };
/******/
/******/ 	// __webpack_public_path__
/******/ 	__webpack_require__.p = "";
/******/
/******/
/******/ 	// Load entry module and return exports
/******/ 	return __webpack_require__(__webpack_require__.s = "../../../../../tmp/0.3467204256847838.WebWorker.js");
/******/ })
/************************************************************************/
/******/ ({

/***/ "../../../../../tmp/0.3467204256847838.WebWorker.js":
/*!********************************************!*\
  !*** /tmp/0.3467204256847838.WebWorker.js ***!
  \********************************************/
/*! no exports provided */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
eval("__webpack_require__.r(__webpack_exports__);\n/* harmony import */ var _home_pablitar_Develop_explorer_kernel_node_modules_decentraland_rpc_lib_common_transports_WebWorker__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./node_modules/decentraland-rpc/lib/common/transports/WebWorker */ \"./node_modules/decentraland-rpc/lib/common/transports/WebWorker.js\");\n\n\nconst imported = __webpack_require__(/*! ./packages/voice-chat-codec/worker.ts */ \"./packages/voice-chat-codec/worker.ts\")\n\nif (imported && imported.__esModule && imported['default']) {\n  new imported['default'](Object(_home_pablitar_Develop_explorer_kernel_node_modules_decentraland_rpc_lib_common_transports_WebWorker__WEBPACK_IMPORTED_MODULE_0__[\"WebWorkerTransport\"])(self))\n}\n\n\n//# sourceURL=webpack:////tmp/0.3467204256847838.WebWorker.js?");

/***/ }),

/***/ "./node_modules/decentraland-rpc/lib/common/transports/WebWorker.js":
/*!**************************************************************************!*\
  !*** ./node_modules/decentraland-rpc/lib/common/transports/WebWorker.js ***!
  \**************************************************************************/
/*! exports provided: WebWorkerTransport */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
eval("__webpack_require__.r(__webpack_exports__);\n/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, \"WebWorkerTransport\", function() { return WebWorkerTransport; });\nfunction WebWorkerTransport(worker) {\n    const api = {\n        onConnect(handler) {\n            worker.addEventListener('message', () => handler(), { once: true });\n        },\n        onError(handler) {\n            worker.addEventListener('error', (err) => {\n                if (err.error) {\n                    handler(err.error);\n                }\n                else if (err.message) {\n                    handler(Object.assign(new Error(err.message), {\n                        colno: err.colno,\n                        error: err.error,\n                        filename: err.filename,\n                        lineno: err.lineno,\n                        message: err.message\n                    }));\n                }\n            });\n        },\n        onMessage(handler) {\n            worker.addEventListener('message', (message) => {\n                handler(message.data);\n            });\n        },\n        sendMessage(message) {\n            worker.postMessage(message);\n        },\n        close() {\n            if ('terminate' in worker) {\n                ;\n                worker.terminate();\n            }\n            else if ('close' in worker) {\n                ;\n                worker.close();\n            }\n        }\n    };\n    return api;\n}\n//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiV2ViV29ya2VyLmpzIiwic291cmNlUm9vdCI6IiIsInNvdXJjZXMiOlsiLi4vLi4vLi4vc3JjL2NvbW1vbi90cmFuc3BvcnRzL1dlYldvcmtlci50cyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFTQSxNQUFNLFVBQVUsa0JBQWtCLENBQUMsTUFBZTtJQUNoRCxNQUFNLEdBQUcsR0FBdUI7UUFDOUIsU0FBUyxDQUFDLE9BQU87WUFDZixNQUFNLENBQUMsZ0JBQWdCLENBQUMsU0FBUyxFQUFFLEdBQUcsRUFBRSxDQUFDLE9BQU8sRUFBRSxFQUFFLEVBQUUsSUFBSSxFQUFFLElBQUksRUFBRSxDQUFDLENBQUE7UUFDckUsQ0FBQztRQUNELE9BQU8sQ0FBQyxPQUFPO1lBQ2IsTUFBTSxDQUFDLGdCQUFnQixDQUFDLE9BQU8sRUFBRSxDQUFDLEdBQWUsRUFBRSxFQUFFO2dCQUNuRCxJQUFJLEdBQUcsQ0FBQyxLQUFLLEVBQUU7b0JBQ2IsT0FBTyxDQUFDLEdBQUcsQ0FBQyxLQUFLLENBQUMsQ0FBQTtpQkFDbkI7cUJBQU0sSUFBSSxHQUFHLENBQUMsT0FBTyxFQUFFO29CQUN0QixPQUFPLENBQ0wsTUFBTSxDQUFDLE1BQU0sQ0FBQyxJQUFJLEtBQUssQ0FBQyxHQUFHLENBQUMsT0FBTyxDQUFDLEVBQUU7d0JBQ3BDLEtBQUssRUFBRSxHQUFHLENBQUMsS0FBSzt3QkFDaEIsS0FBSyxFQUFFLEdBQUcsQ0FBQyxLQUFLO3dCQUNoQixRQUFRLEVBQUUsR0FBRyxDQUFDLFFBQVE7d0JBQ3RCLE1BQU0sRUFBRSxHQUFHLENBQUMsTUFBTTt3QkFDbEIsT0FBTyxFQUFFLEdBQUcsQ0FBQyxPQUFPO3FCQUNyQixDQUFDLENBQ0gsQ0FBQTtpQkFDRjtZQUNILENBQUMsQ0FBQyxDQUFBO1FBQ0osQ0FBQztRQUNELFNBQVMsQ0FBQyxPQUFPO1lBQ2YsTUFBTSxDQUFDLGdCQUFnQixDQUFDLFNBQVMsRUFBRSxDQUFDLE9BQXFCLEVBQUUsRUFBRTtnQkFDM0QsT0FBTyxDQUFDLE9BQU8sQ0FBQyxJQUFJLENBQUMsQ0FBQTtZQUN2QixDQUFDLENBQUMsQ0FBQTtRQUNKLENBQUM7UUFDRCxXQUFXLENBQUMsT0FBTztZQUNqQixNQUFNLENBQUMsV0FBVyxDQUFDLE9BQU8sQ0FBQyxDQUFBO1FBQzdCLENBQUM7UUFDRCxLQUFLO1lBQ0gsSUFBSSxXQUFXLElBQUksTUFBTSxFQUFFO2dCQUV6QixDQUFDO2dCQUFDLE1BQWMsQ0FBQyxTQUFTLEVBQUUsQ0FBQTthQUM3QjtpQkFBTSxJQUFJLE9BQU8sSUFBSSxNQUFNLEVBQUU7Z0JBRTVCLENBQUM7Z0JBQUMsTUFBYyxDQUFDLEtBQUssRUFBRSxDQUFBO2FBQ3pCO1FBQ0gsQ0FBQztLQUNGLENBQUE7SUFFRCxPQUFPLEdBQUcsQ0FBQTtBQUNaLENBQUMiLCJzb3VyY2VzQ29udGVudCI6WyJpbXBvcnQgeyBTY3JpcHRpbmdUcmFuc3BvcnQgfSBmcm9tICcuLi9qc29uLXJwYy90eXBlcydcblxuZXhwb3J0IGludGVyZmFjZSBJV29ya2VyIHtcbiAgdGVybWluYXRlPygpOiB2b2lkXG4gIGNsb3NlPygpOiB2b2lkXG4gIHBvc3RNZXNzYWdlKG1lc3NhZ2U6IGFueSk6IHZvaWRcbiAgYWRkRXZlbnRMaXN0ZW5lcih0eXBlOiAnbWVzc2FnZScgfCAnZXJyb3InLCBsaXN0ZW5lcjogRnVuY3Rpb24sIG9wdGlvbnM/OiBhbnkpOiB2b2lkXG59XG5cbmV4cG9ydCBmdW5jdGlvbiBXZWJXb3JrZXJUcmFuc3BvcnQod29ya2VyOiBJV29ya2VyKTogU2NyaXB0aW5nVHJhbnNwb3J0IHtcbiAgY29uc3QgYXBpOiBTY3JpcHRpbmdUcmFuc3BvcnQgPSB7XG4gICAgb25Db25uZWN0KGhhbmRsZXIpIHtcbiAgICAgIHdvcmtlci5hZGRFdmVudExpc3RlbmVyKCdtZXNzYWdlJywgKCkgPT4gaGFuZGxlcigpLCB7IG9uY2U6IHRydWUgfSlcbiAgICB9LFxuICAgIG9uRXJyb3IoaGFuZGxlcikge1xuICAgICAgd29ya2VyLmFkZEV2ZW50TGlzdGVuZXIoJ2Vycm9yJywgKGVycjogRXJyb3JFdmVudCkgPT4ge1xuICAgICAgICBpZiAoZXJyLmVycm9yKSB7XG4gICAgICAgICAgaGFuZGxlcihlcnIuZXJyb3IpXG4gICAgICAgIH0gZWxzZSBpZiAoZXJyLm1lc3NhZ2UpIHtcbiAgICAgICAgICBoYW5kbGVyKFxuICAgICAgICAgICAgT2JqZWN0LmFzc2lnbihuZXcgRXJyb3IoZXJyLm1lc3NhZ2UpLCB7XG4gICAgICAgICAgICAgIGNvbG5vOiBlcnIuY29sbm8sXG4gICAgICAgICAgICAgIGVycm9yOiBlcnIuZXJyb3IsXG4gICAgICAgICAgICAgIGZpbGVuYW1lOiBlcnIuZmlsZW5hbWUsXG4gICAgICAgICAgICAgIGxpbmVubzogZXJyLmxpbmVubyxcbiAgICAgICAgICAgICAgbWVzc2FnZTogZXJyLm1lc3NhZ2VcbiAgICAgICAgICAgIH0pXG4gICAgICAgICAgKVxuICAgICAgICB9XG4gICAgICB9KVxuICAgIH0sXG4gICAgb25NZXNzYWdlKGhhbmRsZXIpIHtcbiAgICAgIHdvcmtlci5hZGRFdmVudExpc3RlbmVyKCdtZXNzYWdlJywgKG1lc3NhZ2U6IE1lc3NhZ2VFdmVudCkgPT4ge1xuICAgICAgICBoYW5kbGVyKG1lc3NhZ2UuZGF0YSlcbiAgICAgIH0pXG4gICAgfSxcbiAgICBzZW5kTWVzc2FnZShtZXNzYWdlKSB7XG4gICAgICB3b3JrZXIucG9zdE1lc3NhZ2UobWVzc2FnZSlcbiAgICB9LFxuICAgIGNsb3NlKCkge1xuICAgICAgaWYgKCd0ZXJtaW5hdGUnIGluIHdvcmtlcikge1xuICAgICAgICAvLyB0c2xpbnQ6ZGlzYWJsZS1uZXh0LWxpbmU6c2VtaWNvbG9uXG4gICAgICAgIDsod29ya2VyIGFzIGFueSkudGVybWluYXRlKClcbiAgICAgIH0gZWxzZSBpZiAoJ2Nsb3NlJyBpbiB3b3JrZXIpIHtcbiAgICAgICAgLy8gdHNsaW50OmRpc2FibGUtbmV4dC1saW5lOnNlbWljb2xvblxuICAgICAgICA7KHdvcmtlciBhcyBhbnkpLmNsb3NlKClcbiAgICAgIH1cbiAgICB9XG4gIH1cblxuICByZXR1cm4gYXBpXG59XG4iXX0=\n\n//# sourceURL=webpack:///./node_modules/decentraland-rpc/lib/common/transports/WebWorker.js?");

/***/ }),

/***/ "./packages/voice-chat-codec/constants.ts":
/*!************************************************!*\
  !*** ./packages/voice-chat-codec/constants.ts ***!
  \************************************************/
/*! exports provided: OPUS_BITS_PER_SECOND, OPUS_FRAME_SIZE_MS, VOICE_CHAT_SAMPLE_RATE, OPUS_SAMPLES_PER_FRAME, OUTPUT_NODE_BUFFER_SIZE, OUTPUT_NODE_BUFFER_DURATION, INPUT_NODE_BUFFER_SIZE */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
eval("__webpack_require__.r(__webpack_exports__);\n/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, \"OPUS_BITS_PER_SECOND\", function() { return OPUS_BITS_PER_SECOND; });\n/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, \"OPUS_FRAME_SIZE_MS\", function() { return OPUS_FRAME_SIZE_MS; });\n/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, \"VOICE_CHAT_SAMPLE_RATE\", function() { return VOICE_CHAT_SAMPLE_RATE; });\n/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, \"OPUS_SAMPLES_PER_FRAME\", function() { return OPUS_SAMPLES_PER_FRAME; });\n/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, \"OUTPUT_NODE_BUFFER_SIZE\", function() { return OUTPUT_NODE_BUFFER_SIZE; });\n/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, \"OUTPUT_NODE_BUFFER_DURATION\", function() { return OUTPUT_NODE_BUFFER_DURATION; });\n/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, \"INPUT_NODE_BUFFER_SIZE\", function() { return INPUT_NODE_BUFFER_SIZE; });\nconst OPUS_BITS_PER_SECOND = 24000;\nconst OPUS_FRAME_SIZE_MS = 40;\nconst VOICE_CHAT_SAMPLE_RATE = 24000;\nconst OPUS_SAMPLES_PER_FRAME = (VOICE_CHAT_SAMPLE_RATE * OPUS_FRAME_SIZE_MS) / 1000;\nconst OUTPUT_NODE_BUFFER_SIZE = 2048;\nconst OUTPUT_NODE_BUFFER_DURATION = (OUTPUT_NODE_BUFFER_SIZE * 1000) / VOICE_CHAT_SAMPLE_RATE;\nconst INPUT_NODE_BUFFER_SIZE = 2048;\n\n\n//# sourceURL=webpack:///./packages/voice-chat-codec/constants.ts?");

/***/ }),

/***/ "./packages/voice-chat-codec/resampler.ts":
/*!************************************************!*\
  !*** ./packages/voice-chat-codec/resampler.ts ***!
  \************************************************/
/*! exports provided: Resampler */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
eval("__webpack_require__.r(__webpack_exports__);\n/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, \"Resampler\", function() { return Resampler; });\nclass Resampler {\n    constructor(fromSampleRate, toSampleRate, channels) {\n        if (!fromSampleRate || !toSampleRate || !channels) {\n            throw new Error('Invalid settings specified for the resampler.');\n        }\n        this.resampler = null;\n        this.fromSampleRate = fromSampleRate;\n        this.toSampleRate = toSampleRate;\n        this.channels = channels || 0;\n        this.initialize();\n    }\n    initialize() {\n        if (this.fromSampleRate == this.toSampleRate) {\n            this.resampler = (buffer) => {\n                return buffer;\n            };\n            this.ratioWeight = 1;\n        }\n        else {\n            if (this.fromSampleRate < this.toSampleRate) {\n                this.linearInterpolation();\n                this.lastWeight = 1;\n            }\n            else {\n                this.multiTap();\n                this.tailExists = false;\n                this.lastWeight = 0;\n            }\n            this.initializeBuffers();\n            this.ratioWeight = this.fromSampleRate / this.toSampleRate;\n        }\n    }\n    bufferSlice(sliceAmount) {\n        return this.outputBuffer.subarray(0, sliceAmount);\n    }\n    initializeBuffers() {\n        this.outputBufferSize =\n            Math.ceil(((this.inputBufferSize * this.toSampleRate) / this.fromSampleRate / this.channels) * 1.000000476837158203125) +\n                this.channels +\n                this.channels;\n        try {\n            this.outputBuffer = new Float32Array(this.outputBufferSize);\n            this.lastOutput = new Float32Array(this.channels);\n        }\n        catch (error) {\n            this.outputBuffer = Float32Array.of();\n            this.lastOutput = Float32Array.of();\n        }\n    }\n    linearInterpolation() {\n        this.resampler = (buffer) => {\n            let bufferLength = buffer.length, channels = this.channels, outLength, ratioWeight, weight, firstWeight, secondWeight, sourceOffset, outputOffset, outputBuffer, channel;\n            if (bufferLength % channels !== 0) {\n                throw new Error('Buffer was of incorrect sample length.');\n            }\n            if (bufferLength <= 0) {\n                return Float32Array.of();\n            }\n            outLength = this.outputBufferSize;\n            ratioWeight = this.ratioWeight;\n            weight = this.lastWeight;\n            firstWeight = 0;\n            secondWeight = 0;\n            sourceOffset = 0;\n            outputOffset = 0;\n            outputBuffer = this.outputBuffer;\n            for (; weight < 1; weight += ratioWeight) {\n                secondWeight = weight % 1;\n                firstWeight = 1 - secondWeight;\n                this.lastWeight = weight % 1;\n                for (channel = 0; channel < this.channels; ++channel) {\n                    outputBuffer[outputOffset++] = this.lastOutput[channel] * firstWeight + buffer[channel] * secondWeight;\n                }\n            }\n            weight -= 1;\n            for (bufferLength -= channels, sourceOffset = Math.floor(weight) * channels; outputOffset < outLength && sourceOffset < bufferLength;) {\n                secondWeight = weight % 1;\n                firstWeight = 1 - secondWeight;\n                for (channel = 0; channel < this.channels; ++channel) {\n                    outputBuffer[outputOffset++] =\n                        buffer[sourceOffset + (channel > 0 ? channel : 0)] * firstWeight +\n                            buffer[sourceOffset + (channels + channel)] * secondWeight;\n                }\n                weight += ratioWeight;\n                sourceOffset = Math.floor(weight) * channels;\n            }\n            for (channel = 0; channel < channels; ++channel) {\n                this.lastOutput[channel] = buffer[sourceOffset++];\n            }\n            return this.bufferSlice(outputOffset);\n        };\n    }\n    multiTap() {\n        this.resampler = (buffer) => {\n            let bufferLength = buffer.length, outLength, output_variable_list, channels = this.channels, ratioWeight, weight, channel, actualPosition, amountToNext, alreadyProcessedTail, outputBuffer, outputOffset, currentPosition;\n            if (bufferLength % channels !== 0) {\n                throw new Error('Buffer was of incorrect sample length.');\n            }\n            if (bufferLength <= 0) {\n                return Float32Array.of();\n            }\n            outLength = this.outputBufferSize;\n            output_variable_list = [];\n            ratioWeight = this.ratioWeight;\n            weight = 0;\n            actualPosition = 0;\n            amountToNext = 0;\n            alreadyProcessedTail = !this.tailExists;\n            this.tailExists = false;\n            outputBuffer = this.outputBuffer;\n            outputOffset = 0;\n            currentPosition = 0;\n            for (channel = 0; channel < channels; ++channel) {\n                output_variable_list[channel] = 0;\n            }\n            do {\n                if (alreadyProcessedTail) {\n                    weight = ratioWeight;\n                    for (channel = 0; channel < channels; ++channel) {\n                        output_variable_list[channel] = 0;\n                    }\n                }\n                else {\n                    weight = this.lastWeight;\n                    for (channel = 0; channel < channels; ++channel) {\n                        output_variable_list[channel] = this.lastOutput[channel];\n                    }\n                    alreadyProcessedTail = true;\n                }\n                while (weight > 0 && actualPosition < bufferLength) {\n                    amountToNext = 1 + actualPosition - currentPosition;\n                    if (weight >= amountToNext) {\n                        for (channel = 0; channel < channels; ++channel) {\n                            output_variable_list[channel] += buffer[actualPosition++] * amountToNext;\n                        }\n                        currentPosition = actualPosition;\n                        weight -= amountToNext;\n                    }\n                    else {\n                        for (channel = 0; channel < channels; ++channel) {\n                            output_variable_list[channel] += buffer[actualPosition + (channel > 0 ? channel : 0)] * weight;\n                        }\n                        currentPosition += weight;\n                        weight = 0;\n                        break;\n                    }\n                }\n                if (weight === 0) {\n                    for (channel = 0; channel < channels; ++channel) {\n                        outputBuffer[outputOffset++] = output_variable_list[channel] / ratioWeight;\n                    }\n                }\n                else {\n                    this.lastWeight = weight;\n                    for (channel = 0; channel < channels; ++channel) {\n                        this.lastOutput[channel] = output_variable_list[channel];\n                    }\n                    this.tailExists = true;\n                    break;\n                }\n            } while (actualPosition < bufferLength && outputOffset < outLength);\n            return this.bufferSlice(outputOffset);\n        };\n    }\n    resample(buffer) {\n        this.inputBufferSize = buffer.length;\n        if (this.fromSampleRate == this.toSampleRate) {\n            this.ratioWeight = 1;\n        }\n        else {\n            if (this.fromSampleRate < this.toSampleRate) {\n                this.lastWeight = 1;\n            }\n            else {\n                this.tailExists = false;\n                this.lastWeight = 0;\n            }\n            this.initializeBuffers();\n            this.ratioWeight = this.fromSampleRate / this.toSampleRate;\n        }\n        return this.resampler(buffer);\n    }\n}\n\n\n//# sourceURL=webpack:///./packages/voice-chat-codec/resampler.ts?");

/***/ }),

/***/ "./packages/voice-chat-codec/types.ts":
/*!********************************************!*\
  !*** ./packages/voice-chat-codec/types.ts ***!
  \********************************************/
/*! exports provided: RequestTopic, InputWorkletRequestTopic, OutputWorkletRequestTopic, ResponseTopic */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
eval("__webpack_require__.r(__webpack_exports__);\n/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, \"RequestTopic\", function() { return RequestTopic; });\n/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, \"InputWorkletRequestTopic\", function() { return InputWorkletRequestTopic; });\n/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, \"OutputWorkletRequestTopic\", function() { return OutputWorkletRequestTopic; });\n/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, \"ResponseTopic\", function() { return ResponseTopic; });\nvar RequestTopic;\n(function (RequestTopic) {\n    RequestTopic[\"ENCODE\"] = \"ENCODE\";\n    RequestTopic[\"DECODE\"] = \"DECODE\";\n    RequestTopic[\"DESTROY_ENCODER\"] = \"DESTROY_ENCODER\";\n    RequestTopic[\"DESTROY_DECODER\"] = \"DESTROY_ENCODER\";\n})(RequestTopic || (RequestTopic = {}));\nvar InputWorkletRequestTopic;\n(function (InputWorkletRequestTopic) {\n    InputWorkletRequestTopic[\"ENCODE\"] = \"ENCODE\";\n    InputWorkletRequestTopic[\"PAUSE\"] = \"PAUSE\";\n    InputWorkletRequestTopic[\"RESUME\"] = \"RESUME\";\n    InputWorkletRequestTopic[\"ON_PAUSED\"] = \"ON_PAUSED\";\n    InputWorkletRequestTopic[\"ON_RECORDING\"] = \"ON_RECORDING\";\n})(InputWorkletRequestTopic || (InputWorkletRequestTopic = {}));\nvar OutputWorkletRequestTopic;\n(function (OutputWorkletRequestTopic) {\n    OutputWorkletRequestTopic[\"STREAM_PLAYING\"] = \"STREAM_PLAYING\";\n    OutputWorkletRequestTopic[\"WRITE_SAMPLES\"] = \"WRITE_SAMPLES\";\n})(OutputWorkletRequestTopic || (OutputWorkletRequestTopic = {}));\nvar ResponseTopic;\n(function (ResponseTopic) {\n    ResponseTopic[\"ENCODE\"] = \"ENCODE_OUTPUT\";\n    ResponseTopic[\"DECODE\"] = \"DECODE_OUTPUT\";\n})(ResponseTopic || (ResponseTopic = {}));\n\n\n//# sourceURL=webpack:///./packages/voice-chat-codec/types.ts?");

/***/ }),

/***/ "./packages/voice-chat-codec/worker.ts":
/*!*********************************************!*\
  !*** ./packages/voice-chat-codec/worker.ts ***!
  \*********************************************/
/*! no exports provided */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
eval("__webpack_require__.r(__webpack_exports__);\n/* harmony import */ var _types__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./types */ \"./packages/voice-chat-codec/types.ts\");\n/* harmony import */ var _resampler__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./resampler */ \"./packages/voice-chat-codec/resampler.ts\");\n/* harmony import */ var _constants__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./constants */ \"./packages/voice-chat-codec/constants.ts\");\n\n\n\nself.LIBOPUS_WASM_URL = 'libopus.wasm';\nimportScripts('libopus.wasm.js');\nfunction getSampleRate(e) {\n    return e.data.sampleRate ? e.data.sampleRate : 24000;\n}\nconst encoderWorklets = {};\nconst decoderWorklets = {};\nfunction startWorklet(streamId, worklet, outputFunction, messageBuilder) {\n    worklet.working = true;\n    function doWork() {\n        worklet.lastWorkTime = Date.now();\n        let output = outputFunction(worklet);\n        if (output) {\n            if (output instanceof Uint8Array) {\n                output = Uint8Array.from(output);\n            }\n            else {\n                output = Float32Array.from(output);\n            }\n            postMessage(messageBuilder(output, streamId), [output.buffer]);\n            setTimeout(doWork, 0);\n        }\n        else {\n            worklet.working = false;\n        }\n    }\n    setTimeout(doWork, 0);\n}\nonmessage = function (e) {\n    if (e.data.topic === _types__WEBPACK_IMPORTED_MODULE_0__[\"RequestTopic\"].ENCODE) {\n        processEncodeMessage(e);\n    }\n    if (e.data.topic === _types__WEBPACK_IMPORTED_MODULE_0__[\"RequestTopic\"].DECODE) {\n        processDecodeMessage(e);\n    }\n    if (e.data.topic === _types__WEBPACK_IMPORTED_MODULE_0__[\"RequestTopic\"].DESTROY_DECODER) {\n        const { streamId } = e.data;\n        destroyWorklet(decoderWorklets, streamId);\n    }\n    if (e.data.topic === _types__WEBPACK_IMPORTED_MODULE_0__[\"RequestTopic\"].DESTROY_ENCODER) {\n        const { streamId } = e.data;\n        destroyWorklet(encoderWorklets, streamId);\n    }\n};\nfunction processDecodeMessage(e) {\n    const sampleRate = getSampleRate(e);\n    const decoderWorklet = (decoderWorklets[e.data.streamId] = decoderWorklets[e.data.streamId] || {\n        working: false,\n        decoder: new libopus.Decoder(1, sampleRate),\n        lastWorkTime: Date.now(),\n        destroy: function () {\n            this.decoder.destroy();\n        }\n    });\n    decoderWorklet.decoder.input(e.data.encoded);\n    if (!decoderWorklet.working) {\n        startWorklet(e.data.streamId, decoderWorklet, (worklet) => worklet.decoder.output(), (output, streamId) => ({\n            topic: _types__WEBPACK_IMPORTED_MODULE_0__[\"ResponseTopic\"].DECODE,\n            streamId,\n            samples: toFloat32Samples(output)\n        }));\n    }\n}\nfunction processEncodeMessage(e) {\n    const sampleRate = getSampleRate(e);\n    const encoderWorklet = (encoderWorklets[e.data.streamId] = encoderWorklets[e.data.streamId] || {\n        working: false,\n        encoder: new libopus.Encoder(1, sampleRate, _constants__WEBPACK_IMPORTED_MODULE_2__[\"OPUS_BITS_PER_SECOND\"], _constants__WEBPACK_IMPORTED_MODULE_2__[\"OPUS_FRAME_SIZE_MS\"], true),\n        lastWorkTime: Date.now(),\n        destroy: function () {\n            this.encoder.destroy();\n        }\n    });\n    const samples = toInt16Samples(resampleIfNecessary(e.data.samples, e.data.sampleRate, e.data.inputSampleRate));\n    encoderWorklet.encoder.input(samples);\n    if (!encoderWorklet.working) {\n        startWorklet(e.data.streamId, encoderWorklet, (worklet) => worklet.encoder.output(), (output, streamId) => ({ topic: _types__WEBPACK_IMPORTED_MODULE_0__[\"ResponseTopic\"].ENCODE, streamId: streamId, encoded: output }));\n    }\n}\nfunction resampleIfNecessary(floatSamples, targetSampleRate, inputSampleRate) {\n    if (inputSampleRate && inputSampleRate !== targetSampleRate) {\n        const resampler = new _resampler__WEBPACK_IMPORTED_MODULE_1__[\"Resampler\"](inputSampleRate, targetSampleRate, 1);\n        return resampler.resample(floatSamples);\n    }\n    else {\n        return floatSamples;\n    }\n}\nfunction toInt16Samples(floatSamples) {\n    return Int16Array.from(floatSamples, (floatSample) => {\n        let val = Math.floor(32767 * floatSample);\n        val = Math.min(32767, val);\n        val = Math.max(-32768, val);\n        return val;\n    });\n}\nfunction toFloat32Samples(intSamples) {\n    return Float32Array.from(intSamples, (intSample) => {\n        let floatValue = intSample >= 0 ? intSample / 32767 : intSample / 32768;\n        return Math.fround(floatValue);\n    });\n}\nfunction destroyWorklet(worklets, workletId) {\n    var _a;\n    (_a = worklets[workletId]) === null || _a === void 0 ? void 0 : _a.destroy();\n    delete worklets[workletId];\n}\n\n\n//# sourceURL=webpack:///./packages/voice-chat-codec/worker.ts?");

/***/ })

/******/ })));