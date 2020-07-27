define([],
	function () {


		var Header = function (container) {

			this._element = document.createElement('div');
			this._element.id = 'HeaderElement';

			this._controlsContainer = document.createElement('h2');
			this._controlsContainer.id = 'header-title';
			this._controlsContainer.style.clear = "both";
			this._controlsContainer.style.textAlign = "center";
			this._controlsContainer.innerHTML = "Melody Maker";
			this._element.appendChild(this._controlsContainer);

			container.appendChild(this._element);
		};

		

		return Header;
	});