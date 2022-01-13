console.log("here");
var dataTabe;


$(document).ready(function () {
    var search = window.location.search;
    if (search.includes("inprocess")) {
        loadDatatable("inprocess")
    }
    loadDatatable();
});


function loadDatatable(status) {

    dataTable = $("#OrderData").DataTable({
        searching: false,
        ajax: {
            "url": "/admin/order/getall"
        },
        columns: [
            { data: "id" },
            { data: "applicationUser.name" },
            { data: "applicationUser.phoneNumber" },
            { data: "applicationUser.email" },
            { data: "orderStatus" },
            { data: "totalAmount" },
            {
                data: "id",
                render: function (data, type, row, meta) {
                    return `
                    <a class="btn btn-primary" style="width:100px" href="/${data}">
                             <i class="bi bi-pencil-square"></i> &nbsp; Details
                            </a>
                `
                }
            }
        ]
    });
}