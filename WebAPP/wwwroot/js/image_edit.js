$(document).ready(function () {
    function readURL(input) {
        if (input.files && input.files[0]) {
            var reader = new FileReader();

            reader.onload = function (e) {
                // Si ya hay una imagen cargada, la reempláza
                if ($('.productviewsimg img').length > 0) {
                    $('.productviewsimg img').attr('src', e.target.result);
                } else {
                    // Si no hay una imagen cargada, añade la nueva imagen
                    $('.productviewsimg').html('<img src="' + e.target.result + '" alt="img">');
                }
            }
            reader.readAsDataURL(input.files[0]);
        }
    }

    $("#Image").change(function () {
        readURL(this);
    });
});