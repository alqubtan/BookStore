var dataTable;

$(document).ready(function () {
    var url = window.location.search
    if (url.includes("Pending")) {
        loadDataTable("Pending");
        return
    } else if (url.includes("Approved")) {
        loadDataTable("Approved");
        return

    }else if (url.includes("InProcess")) {
        loadDataTable("InProcess");
        return

    } else if (url.includes("Completed")) {
        loadDataTable("Completed");
        return
    } else {
        loadDataTable("All");

    }
    
    
});

function loadDataTable(status) {
    dataTable = $('#ordersTable').DataTable({
        "ajax": {
            "url": "/Admin/Order/GetAll?status=" + status
        },
        "columns": [
            { "data": "id", "width": "5%" },
            { "data": "name", "width": "15%" },
            { "data": "phoneNumber", "width": "15%" },
            { "data": "applicationUser.email", "width": "15%" },
            { "data": "orderStatus", "width": "15%" },
            { "data": "orderTotal", "width": "15%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                         <div class="w-75 btn-group" role="group">
                             <a href="/Admin/Order/Details?orderId=${data}"
                                class="btn btn-primary mx-2">
                                <i class="bi bi-info-square"></i>
                             </a>

                         </div>`;
                },
                "width": "5%"
            },
        ]

    });
}

