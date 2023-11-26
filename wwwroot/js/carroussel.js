document.addEventListener('DOMContentLoaded', function () {

    const sliderMe = () => {
        let currentPosition = 0,
            sliderItem = document.querySelectorAll('.slider-item'),
            sliderItemWidth = window
                .getComputedStyle(sliderItem[0])
                .flexBasis.match(/\d+\.?\d+/g),
            sliderInner = document.querySelector('.slider-inner');

        const control = {
            next: document.querySelector('#next'),
            slideNext() {
                currentPosition += parseFloat(sliderItemWidth);
                if (currentPosition > limitPosition) {
                    currentPosition = 0;
                }
                sliderInner.style.right = currentPosition + '%';
            },
            prev: document.querySelector('#prev'),
            slidePrev() {
                currentPosition -= parseFloat(sliderItemWidth);
                if (currentPosition < 0) {
                    currentPosition = limitPosition;
                }
                sliderInner.style.right = currentPosition + '%';
            }
        };

        const limitPosition = sliderItemWidth * (sliderItem.length - Math.floor(100 / sliderItemWidth));

        control.next.addEventListener('click', control.slideNext);
        control.prev.addEventListener('click', control.slidePrev);

        window.addEventListener("resize", function () {
            currentPosition = 0;
            document.querySelector('.slider-inner').style.right = currentPosition + '%';
        });
    };

    sliderMe();

    window.addEventListener("resize", sliderMe);
});
