// ================== CANVAS SETUP ==================
var canvas = document.getElementById('canvas');
var context = canvas.getContext('2d');

// ================== CONSTANTS ==================
const FRAME_PADDING_X = 40;
const DISTANCE_RODS = 60;
const TOP_MARGIN = 60;
const NUMBER_HEIGHT = 20;
const LEFT_MARGIN = 10;

const FRAME_LINE_WIDTH = 10;
const ROD_LINE_WIDTH = 6;

const BEAD_WIDTH = 56;
const BEAD_HEIGHT = 30;
const BEAD_GAP = 2;

const HEAVEN = BEAD_HEIGHT * 2 + FRAME_LINE_WIDTH;
const EARTH = BEAD_HEIGHT * 5;
const HEIGHT = HEAVEN + EARTH + FRAME_LINE_WIDTH;

// ================== COLORS ==================
var frameColor = 'black';
var beadColor = 'ivory';
var activeColor = 'sienna';
var ROD_STROKE_STYLE = 'rgba(212,85,0,0.5)';
var BEAD_STROKE = 'black';

// ================== DECIMAL MODE ==================
var decimalMode = false;
var decimalRodIndex = 10; // rod position of decimal separator (0-based)

// ================== GLOBALS ==================
var top_frame;
var abacus = null;

// ================== THEMES ==================
var themeSelect = document.getElementById('themeSelect');

const THEMES = {
  classic: {
    frameColor: 'black',
    beadColor: 'ivory',
    activeColor: 'sienna',
    rodColor: 'rgba(212,85,0,0.5)'
  },
  dark: {
    frameColor: '#eaeaea',
    beadColor: '#444',
    activeColor: '#ff9f43',
    rodColor: '#999'
  },
  light: {
    frameColor: '#333',
    beadColor: '#fafafa',
    activeColor: '#ff6b6b',
    rodColor: '#777'
  },
  wood: {
    frameColor: '#3b1f00',
    beadColor: '#e8c89b',
    activeColor: '#7a3b00',
    rodColor: '#dd8d1dff'
  }
};

function applyTheme(name) {
  const t = THEMES[name];

  // Canvas colors
  frameColor = t.frameColor;
  beadColor = t.beadColor;
  activeColor = t.activeColor;
  ROD_STROKE_STYLE = t.rodColor;

  // Page colors
  document.body.className = 'theme-' + name;

  abacus.draw();
}


// ================== CONSTRUCTORS ==================
function Abacus(numberOfRods) {
	this.numberOfRods = numberOfRods;
	this.rods = [];

	for (let i = 0; i < numberOfRods; i++) {
		let beads = [];
		let rod = new Rod(i + 1, beads);
		beads.push(new Bead(rod, true, 0));
		for (let j = 1; j <= 4; j++) {
			beads.push(new Bead(rod, false, j));
		}
		this.rods.push(rod);
	}

	this.width =
		FRAME_PADDING_X * 2 +
		(this.numberOfRods - 1) * DISTANCE_RODS;

	this.updateMultipliers();
}

function Rod(position, beads) {
	this.position = position;
	this.beads = beads;
	this.value = 0;
	this.multiplier = 1;
}

function Bead(rod, heaven, order) {
	this.rod = rod;
	this.heaven = heaven;
	this.order = order;
	this.active = false;
}

function Point(x, y) {
	this.x = x;
	this.y = y;
}

// ================== ABACUS ==================
Abacus.prototype.updateMultipliers = function () {
	const total = this.numberOfRods;

	this.rods.forEach((rod, i) => {
		if (!decimalMode) {
			rod.multiplier = Math.pow(10, total - i - 1);
		} else {
			rod.multiplier = Math.pow(10, decimalRodIndex - i);
		}
	});
};

Abacus.prototype.getTotalValue = function () {
	let sum = 0;
	this.rods.forEach(r => {
		sum += r.value * r.multiplier;
	});
	return sum;
};

Abacus.prototype.draw = function () {
	top_frame = TOP_MARGIN + NUMBER_HEIGHT;
	canvas.width = this.width + LEFT_MARGIN * 2;
	canvas.height = top_frame + HEIGHT + 40;
	context.clearRect(0, 0, canvas.width, canvas.height);

	this.drawRods();
	this.drawFrame();
	this.drawDecimalMarker();
	this.drawTotalValue();
};

Abacus.prototype.drawFrame = function () {
	context.save();
	context.strokeStyle = frameColor;
	context.lineWidth = FRAME_LINE_WIDTH;

	context.strokeRect(
		LEFT_MARGIN,
		top_frame,
		this.width,
		HEIGHT
	);

	context.beginPath();
	context.moveTo(LEFT_MARGIN, top_frame + HEAVEN);
	context.lineTo(LEFT_MARGIN + this.width, top_frame + HEAVEN);
	context.stroke();

	context.restore();
};

Abacus.prototype.drawDecimalMarker = function () {
	if (!decimalMode) return;

	const x =
		LEFT_MARGIN +
		FRAME_PADDING_X +
		decimalRodIndex * DISTANCE_RODS +
		DISTANCE_RODS / 2;

	context.save();
	context.strokeStyle = 'red';
	context.lineWidth = 3;
	context.beginPath();
	context.moveTo(x, top_frame - 15);
	context.lineTo(x, top_frame + HEIGHT + 15);
	context.stroke();
	context.restore();
};

Abacus.prototype.drawTotalValue = function () {
	context.save();
	context.fillStyle = '#000';
	context.font = 'bold 24px Courier New, monospace';
	context.textAlign = 'center';

	let value = this.getTotalValue();
	let text = decimalMode ? value.toFixed(decimalRodIndex) : value.toString();

	context.fillText(
		text,
		canvas.width / 2,
		canvas.height - 10
	);

	context.restore();
};

Abacus.prototype.drawRods = function () {
	this.rods.forEach(r => r.draw());
};

// ================== ROD ==================
Rod.prototype.evalXPos = function () {
	return (
		LEFT_MARGIN +
		FRAME_PADDING_X +
		(this.position - 1) * DISTANCE_RODS
	);
};

Rod.prototype.drawValue = function () {
	context.save();
	context.fillStyle = '#7a0026';
	context.font = 'bold 24px Courier New, monospace';
	context.textAlign = 'center';
	context.fillText(this.value, this.evalXPos(), TOP_MARGIN);
	context.restore();
};

Rod.prototype.draw = function () {
	context.save();
	context.strokeStyle = ROD_STROKE_STYLE;
	context.lineWidth = ROD_LINE_WIDTH;

	context.beginPath();
	context.moveTo(this.evalXPos(), top_frame);
	context.lineTo(this.evalXPos(), top_frame + HEIGHT);
	context.stroke();

	context.restore();

	this.beads.forEach(b => b.draw());
	this.drawValue();
};

// ================== BEAD ==================
Bead.prototype.evalPosition = function () {
	const x = this.rod.evalXPos();
	let y;

	if (this.heaven) {
		y = this.active
			? top_frame + HEAVEN - BEAD_HEIGHT / 2 - BEAD_GAP
			: top_frame + BEAD_HEIGHT / 2 + BEAD_GAP;
	} else {
		const idx = this.active ? this.order - 1 : this.order;
		y =
			top_frame +
			HEAVEN +
			idx * (BEAD_HEIGHT + BEAD_GAP) +
			BEAD_HEIGHT / 2;
	}

	return new Point(x, y);
};

Bead.prototype.getPoints = function () {
	const c = this.evalPosition();
	return [
		new Point(c.x - BEAD_WIDTH / 2, c.y),
		new Point(c.x - BEAD_WIDTH / 6, c.y - BEAD_HEIGHT / 2),
		new Point(c.x + BEAD_WIDTH / 6, c.y - BEAD_HEIGHT / 2),
		new Point(c.x + BEAD_WIDTH / 2, c.y),
		new Point(c.x + BEAD_WIDTH / 6, c.y + BEAD_HEIGHT / 2),
		new Point(c.x - BEAD_WIDTH / 6, c.y + BEAD_HEIGHT / 2),
	];
};

Bead.prototype.draw = function () {
	context.save();
	context.fillStyle = this.active ? activeColor : beadColor;
	context.strokeStyle = BEAD_STROKE;

	const pts = this.getPoints();
	context.beginPath();
	context.moveTo(pts[0].x, pts[0].y);
	for (let i = 1; i < pts.length; i++) context.lineTo(pts[i].x, pts[i].y);
	context.closePath();
	context.fill();
	context.stroke();
	context.restore();
};

// ================== INTERACTION ==================
function getBead(rod, heaven, order) {
	return rod.beads.find(b => b.heaven === heaven && b.order === order);
}

function clickedBead(bead) {
	if (bead.heaven) {
		bead.active = !bead.active;
		bead.rod.value += bead.active ? 5 : -5;
	} else {
		if (bead.active) {
			bead.active = false;
			bead.rod.value--;
			for (let i = bead.order + 1; i <= 4; i++) {
				const b = getBead(bead.rod, false, i);
				if (b.active) {
					b.active = false;
					bead.rod.value--;
				}
			}
		} else {
			bead.active = true;
			bead.rod.value++;
			for (let i = 1; i < bead.order; i++) {
				const b = getBead(bead.rod, false, i);
				if (!b.active) {
					b.active = true;
					bead.rod.value++;
				}
			}
		}
	}
	abacus.draw();
}

canvas.onclick = function (e) {
	const rect = canvas.getBoundingClientRect();
	const x = e.clientX - rect.left;
	const y = e.clientY - rect.top;

	for (const rod of abacus.rods) {
		for (const bead of rod.beads) {
			const pts = bead.getPoints();
			context.beginPath();
			context.moveTo(pts[0].x, pts[0].y);
			for (let i = 1; i < pts.length; i++) context.lineTo(pts[i].x, pts[i].y);
			context.closePath();
			if (context.isPointInPath(x, y)) {
				clickedBead(bead);
				return;
			}
		}
	}
};

// ================== CONTROLS ==================
var rodsSelect = document.getElementById('numberOfRods');
var resetBtn = document.getElementById('reset');
var decimalToggle = document.getElementById('decimalToggle');

resetBtn.onclick = function () {
	abacus = new Abacus(abacus.numberOfRods);
	abacus.draw();
};

rodsSelect.onchange = function () {
	abacus = new Abacus(parseInt(this.value, 10));
	abacus.draw();
};

decimalToggle.onchange = function () {
	decimalMode = this.checked;
	abacus.updateMultipliers();
	abacus.draw();
};

themeSelect.onchange = function () {
  applyTheme(this.value);
};


// ================== INIT ==================
abacus = new Abacus(15);
abacus.draw();
applyTheme('classic');
