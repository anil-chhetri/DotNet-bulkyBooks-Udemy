var dataTable;

function loadDataTable() {
    console.log('here');
    dataTable = $("#dataTable").DataTable({
        "searching": false,
        "ajax": {
            "url" : "/Admin/Product/GetAll"
        },
        "columns": [
            {"data" : "title"},
            {"data" : "isbn"},
            {"data" : "price"},
            { "data": "author" },
            { "data": "category.name" },
            {
                "data": "id",
                "render": function (data) {
                    return  `
                        <div class="w-75 btn-group" role="group">
                            <a href="/Admin/Product/Upsert?id=${data}" class="mx-2 btn btn-primary" style="width:150px">
                                <i class="bi bi-pencil-square"></i> Edit
                            </a>
                            <a onClick="Delete('/Admin/Product/DeletePOST/?id=${data}')" class="mx-2 btn btn-danger" style="width:150px">
                                <i class="bi bi-trash"></i>Delete
                            </a>
                        </div>
                    `
                }
            },

        ]
    });
}



$(document).ready(function () {
    loadDataTable();
});


function Delete(url) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: "DELETE",
                success: function (data) {
                    if (data.success) {
                        dataTable.ajax.reload();
                        Swal.fire(
                            'Deleted!',
                            'Product Deleted.',
                            'success'
                        )
                        toastr.success(data.message);
                    }
                    else {
                        toastr.error(data.message);
                    }
                },
                error: function (data) {
                    if (!data.success) {
                        toastr.error("some thing went wrong");
                    }
                }

            });
        }
    })
}