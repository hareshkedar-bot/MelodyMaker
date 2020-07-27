/**
 * Copyright 2016 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

require(['domready',  'grid/Grid', 'interface/Bottom', 'sound/Sequencer', 
	'Tone/core/Transport', 'sound/Player', 'data/Config'],

	function (domReady, Grid, Bottom, Sequencer, Transport, Player, Config) {
	//domReady
	(function () {

		var grid = null;
		var modal = document.getElementById("myModal");
		modal.style.display = "block";
		document.getElementsByClassName("okbutton")[0].onclick = function () {
			var mobileNumber = document.getElementById("mobilenumber").value;
			if (mobileNumber != "") {
				if (Config.inputModified) {
					grid.updateClick();
					Config.defaultInput = mobileNumber;
					grid.defaultClick();
				}
				else {
					Config.defaultInput = mobileNumber;
					loadDom();
				}
				modal.style.display = "none";
				document.getElementById("mobilenumber").value = "";
			}
			else {
				alert("kindly input mobile number");
			}
		}
		function loadDom() {
			window.parent.postMessage("loaded", "*");

			grid = new Grid(document.body);
			var bottom = new Bottom(document.body);

			bottom.onDirection = function (dir) {
				grid.setDirection(dir);
			};

			var player = new Player();

			var seq = new Sequencer(function (time, step) {
				var notes = grid.select(step);
				player.play(notes, time);
			});

			grid.onNote = function (note) {
				player.tap(note);
			};

			Transport.on('stop', function () {
				grid.select(-1);
			});

			//send the ready message to the parent
			var isIOS = /iPad|iPhone|iPod/.test(navigator.userAgent) && !window.MSStream;
			var isAndroid = /Android/.test(navigator.userAgent) && !window.MSStream;

			//full screen button on iOS
			//if (isIOS || isAndroid){
			//	make a full screen element and put it in front
			//	var iOSTapper = document.createElement("div");
			//	iOSTapper.id = "iOSTap";
			//          document.body.appendChild(iOSTapper);
			//	new StartAudioContext(Transport.context, iOSTapper).then(function() {
			//		iOSTapper.remove();
			//		window.parent.postMessage('ready','*');
			//	});
			//} else {
			window.parent.postMessage('ready', '*');
			//}
		}
	})();
	});
