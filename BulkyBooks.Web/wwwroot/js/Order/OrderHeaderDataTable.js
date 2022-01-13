console.log("here");
var dataTabe;


$(document).ready(function () {
    var search = window.location.search;

    if (search.includes("inprocess")) {
        console.log("inprocess")
        loadDatatable("inprocess")
    } else if (search.includes("pending")) {
        console.log("pending")

        loadDatatable("pending")
    } else if (search.includes("completed")) {
        console.log("complted")

        loadDatatable("completed")
    } else {
        loadDatatable("")
    }
});


function loadDatatable(status) {

    dataTable = $("#OrderData").DataTable({
        searching: false,
        ajax: {
            "url": "/admin/order/getall?status=" + status
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
                    <a class="btn btn-primary" style="width:100px" href="/admin/order/details/${data}">
                             <i class="bi bi-pencil-square"></i> &nbsp; Details
                            </a>
                `
                }
            }
        ]
    });
}