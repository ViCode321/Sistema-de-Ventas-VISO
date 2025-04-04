function SoloNumeros(event) {
    var keynum;

    if (window.event) {
        keynum = event.keyCode;
    }
    else {
        keynum = event.which;
    }

    if ((keynum > 47 && keynum < 58) || (keynum == 8) || (keynum == 13)) {
        return true;
    }
    else {
        Swal.fire({
            title: "Error",
            text: 'Solo puede ingresar números',
            icon: "warning",
            confirmButtonColor: "#d33",
            confirmButtonText: "OK"
        });
        return false;
    }
}
