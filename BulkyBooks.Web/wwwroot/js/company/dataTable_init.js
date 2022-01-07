var dataTable;

function loadDataTable() {
    dataTable = $("#CmpTableData").DataTable({
        "searching": false,
        ajax: {
            url: "/Admin/company/getcompany",
            dataSrc:'data'
        },
        columns :[
            {data: 'name'},
            { data: 'streetAddress'},
            { data: 'city'},
            { data: 'state'},
            { data: 'postalCode'},
            { data: 'phoneNumber'},
            {
                data: 'id',
                render: function (data, type, row, meta) {
                    return `<a class="btn btn-primary" style="width:100px" href="/Admin/company/upsert/${data}">
                             <i class="bi bi-pencil-square"></i> &nbsp; Edit
                            </a>
                            <a class="btn btn-danger" onClick="Delete('/Admin/company/delete/${data}')" style="width:100px">
                                <i class="bi bi-trash2-fill"></i> &nbsp; Delete
                            </a>
                            `
                }
            },
        ]
    });
}


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
                type: "Delete",
                success: function (result) {
                    if (result.success) {
                        dataTable.ajax.reload();
                        Swal.fire(
                            'Deleted!',
                            result.message,
                            'success'
                        )
                        toastr.success(result.message);

                    } else {
                        Swal.fire({
                            icon: 'error',
                            title: 'Oops...',
                            text: result.message,
                        })
                        toastr.error(result.message);
                    }
                }
            });
            
        }
    })
}

$(document).ready(
    loadDataTable
);