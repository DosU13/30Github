document.addEventListener("DOMContentLoaded", function () {
    const videos = document.querySelectorAll(".autoplay-video");
    const buttons = document.querySelectorAll(".play-btn");

    function handleIntersection(entries, observer) {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.play();
            } else {
                entry.target.pause();
            }
        });
    }

    const observer = new IntersectionObserver(handleIntersection, { threshold: 0.5 });
    videos.forEach(video => observer.observe(video));

    buttons.forEach((button, index) => {
        button.addEventListener("click", () => {
            if (videos[index].paused) {
                videos[index].play();
            } else {
                videos[index].pause();
            }
        });
    });
});