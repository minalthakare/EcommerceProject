function Delete(url) {
    swal({
        title: "Click Ok if you want to delete the item ",
        text: "if click  ok will be deleted for parmanently",
        icon: "warning",
        buttons: true,
        dangerMode: true
    }).then((willDelete) => {
        if (willDelete) {
            $.ajax({
                type: "DELETE",
                url: url,
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);

                        setTimeout(reloadPage(), 3000);
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            })
        }
    });
}


function reloadPage() {
    window.location.reload();
}

