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

require(['domready',  'grid/Grid',  'sound/Sequencer', 
	'Tone/core/Transport', 'sound/Player', 'data/Config', 'interface/Header'],

	function (domReady, Grid,  Sequencer, Transport, Player, Config, Header) {
	//domReady
	(function () {

		var grid = null;
		var modal = document.getElementById("myModal");
		modal.style.display = "block";

		document.getElementsByClassName("okbutton")[0].onclick = function () {
			var mobileNumber = document.getElementById("mobilenumber").value;
			if (mobileNumber == "")
				document.getElementById("alertmsg").style.display = "block";
			else if (mobileNumber != "" && mobileNumber.length != 10)
				document.getElementById("alertmsg").style.display = "block";
			else {
				document.getElementById("cover-spin").style.display = "block";
				document.getElementById("alertmsg").style.display = "none";
				var xhr = new XMLHttpRequest();
				xhr.open('POST', encodeURI("/api/audio"), true);
				xhr.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
				//xhr.setRequestHeader('Content-Type', 'application/json');
				xhr.responseType = 'blob';
				xhr.setRequestHeader("cache-control", "no-cache");
				xhr.onload = function (evt) {
					var blob = new Blob([xhr.response], { type: 'audio/mpeg' });
					var objectUrl = URL.createObjectURL(blob);
					var audio = document.getElementById("audioElement");
					audio.src = objectUrl;
					var downloadlink = document.getElementById("downloadlink");
					downloadlink.src = objectUrl;
					downloadlink.setAttribute('download', mobileNumber +"_file.mp3");
					audio.onload = function (evt) {
						URL.revokeObjectURL(objectUrl);
					};
					Config.disableClick = false;
					grid.defaultClick();
					Config.defaultInput = mobileNumber;
					grid.updateClick();
					modal.style.display = "none";
					document.getElementById("mobilenumber").value = "";
					document.getElementById("cover-spin").style.display = "none";
				};
				
				var data = "phoneNumber=" + mobileNumber + "";
				xhr.send(data);
			}
		}
		loadDom();
		function loadDom() {
			window.parent.postMessage("loaded", "*");

			var header = new Header(document.body);
			grid = new Grid(document.body);
			//var bottom = new Bottom(document.body);

			//bottom.onDirection = function (dir) {
			//	grid.setDirection(dir);
			//};

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

			var audioElement = document.createElement("audio");
			audioElement.id = 'audioElement';
			audioElement.type = "audio/mpeg";
			audioElement.loop = true;
			audioElement.hidden = true;
			document.body.appendChild(audioElement);

			playClicked = function (element) {
				if (playButton.classList.contains("Playing")) {
					playButton.classList.remove('Playing');
					playButton.classList.add('icon-svg_play');
					playButton.classList.remove('icon-svg_pause');
					audioElement.pause();
				}
				else {
					playButton.classList.add('Playing');
					playButton.classList.remove('icon-svg_play');
					playButton.classList.add('icon-svg_pause');
					audioElement.play();
					
				}
			}

			var link = document.createElement('a');
			link.id = "downloadlink";
			link.classList.add('Button');
			link.classList.add('icon-svg_download');
			link.style.textDecoration = "none";
			link.href = "";
			link.setAttribute('download', "DownloadedFilenameHere.mp3");
			document.body.appendChild(link);

			var playButton = document.createElement('div');
			playButton.id = 'PlayButton';
			playButton.classList.add('Button');
			playButton.classList.add("icon-svg_play");
			document.body.appendChild(playButton);
			playButton.addEventListener('click', playClicked.bind(this));


			onReEnterClick = function () {
				Config.inputModified = true;
				var modal = document.getElementById("myModal");
				modal.style.display = "block";
			}

			var reenter = document.createElement('button');
			reenter.id = "Reenter";
			reenter.innerHTML = "Re-enter Phone";
			reenter.addEventListener("click", onReEnterClick.bind());
			document.body.appendChild(reenter);

			

			//var sourceElement = document.createElement("source");
			//sourceElement.id = 'audioElement';
			//document.getElementById("audioElementID").appendChild(sourceElement);
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
