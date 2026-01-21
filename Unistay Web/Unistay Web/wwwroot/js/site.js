// Main site logic
$(document).ready(function () {
  // Initialize tooltips/popovers if needed
  // $('[data-toggle="tooltip"]').tooltip();

  // Smooth scrolling for anchor links
  $('a[href^="#"]').on('click', function (e) {
    e.preventDefault();
    var target = this.hash;
    var $target = $(target);
    if ($target.length) {
      $('html, body').stop().animate({
        'scrollTop': $target.offset().top - 70
      }, 800, 'swing');
    }
  });
});
