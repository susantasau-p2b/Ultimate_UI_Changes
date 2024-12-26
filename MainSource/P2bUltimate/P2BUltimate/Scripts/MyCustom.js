$(document).ready(function () {
    var selected = [];
    var dt = $("#MyTable").DataTable({
        "jQueryUI": true,
        "bServerSide": true,
        "sAjaxSource": "../Geostruct/AjaxHandler12",
        "bProcessing": true,
        "scrollY": 250,
        "scrollCollapse": true,
        responsive: true,
        "sAjaxDataProp": "aadata",
        "dom": '<T><C><"H"<"clear">lfr>t<"F"ip>',
        "oTableTools": {
            "sSwfPath": "swf/copy_csv_xls_pdf.swf",
            "aButtons": [
            {
                "sExtends": "collection",
                "sButtonText": "Save To",
                "aButtons": ["pdf", "xls", "csv"]
            },
            "copy"
            ]
        },
        "columns": [
            {
                "class": "details-control",
                "orderable": false,
                "defaultContent": "",
                "bSearchable": false,
                "bSortable": false
                
            },
            { "sName": "Company_ID"},
            { "sName": "Company_Name" },
            { "sName": "Company_Description" },
            { "sName": "Company_City" },
            { "sName": "Company_Country" }
        ],
        "order": [[1, 'asc']],
        "rowCallback": function (row, data) {
            if($.inArray('aadata',selected) !== -1){
                $(row).addClass('selected');
            }
        }
    });
    //toast message..!
   $().toastmessage('showToast', {
        text: 'Data is About to Loaded .... !',
        sticky: false,
        position: 'top-center',
        type: 'notice'
    });

    //ColReorder Columns...!
    new $.fn.DataTable.ColReorder(dt);

    //Row Selections in DataTable..!
    $('#MyTable tbody').on('click', 'tr', function () {
        var id = this.id;
        var index = $.inArray(id,selected);
        if ($(this).hasClass('selected')) {
            $(this).removeClass('selected')
        }
        else {
            $('#MyTable').DataTable().$('tr.selected').removeClass('selected');
            $(this).addClass('selected');
        }
    });

    //Add Event Listener...! For Details Row
    var detailRows = [];
    $('#MyTable tbody').on('click', 'td.details-control', function () {
        var tr = $(this).parent('tr');
        var row = dt.row(tr);
        var idx = $(tr.find("td:eq(1)")).text();
        if (row.child.isShown()) {
            tr.removeClass('details');
            row.child.hide();
        }
        else {
            tr.addClass('details', function () {
                $().toastmessage('showToast',{
                    text: 'Please Wait Data is Loading for the Company ID ' + idx + '',
                    sticky: false,
                    position: 'top-center',
                    type:'notice'
                });
                $('.loading').dialog({
                    modal: true,
                    hide: {
                        effect: "clip",
                        duration: 500
                    }
                });
            });
            $.ajax({
                url: 'Home/GiveallDetails',
                type: 'POST',
                datatype: 'json',
                data: { x: idx },
                success: function (res) {
                    var value = '<table class="table" id="table"><tr><th>Company_ID</th><th>Company_Name</th><th>Company_Description</th><th>Company_City</th><th>Company_Country</th></tr>'
                    $.each(res, function (i, k) {
                        value += '<tr>' +
                            '<td>' + k.Company_ID + '</td>' +
                            '<td>' + k.Company_Name + '</td>' +
                            '<td>' + k.Company_Description + '</td>' +
                            '<td>' + k.Company_City + '</td>' +
                            '<td>' + k.Company_Country + '</td>' +
                            '</tr>';
                    });
                    value += "</table>";
                    setTimeout(function () {
                        $('.loading').dialog("close");
                    },500);
                    row.child(value).show();
                }
            });
        }
    });
});
//this method is used for visiblity of an column true or false.          
//"columnDefs": [
//    {
//        "targets": [0],
//        "visible": false,
//        "searchable": false
//    }
//],

//this code is used for, Multiple row Selection
//if (index === -1) {
//    selected.push(id);
//} else {
//    selected.splice(index, 1);
//}
//$(this).toggleClass('selected');