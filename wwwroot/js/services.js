
let card = document.querySelectorAll(".card");
for (let i = 1; i <= card.length; i++) {
	document.documentElement.style.setProperty("--face_height_" + i + "", document.querySelector(".card-container .card:nth-child(" + i + ") .face-2").scrollHeight + "px");
}			