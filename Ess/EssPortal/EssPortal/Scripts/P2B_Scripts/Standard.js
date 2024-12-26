(function (factory) {
    if (typeof define === "function" && define.amd) {
        define(["jquery"], factory);
    } else {
        factory(jQuery);
    }
}
    (function ($) {
        "use strict";
        $.fn.P2BDatePicker = function () {
            this.datetimepicker({
                lang: 'en',
                timepicker: false,
                format: 'd/m/Y',
                formatDate: 'dd/MM/yyyy'
            });
        };
        $.fn.P2BTimePicker = function () {
            /*
         Dependancy datetimepickerjs
         */
            this.datetimepicker({
                datepicker: false,
                format: 'H:i',
                step: 1 //Use this as minutes as per steps.
            });
        };
        $.fn.P2BDateInlinePicker = function () {
            this.datetimepicker({
                lang: 'en',
                inline: true,
                timepicker: false,
                format: 'd/m/Y',
                formatDate: 'dd/MM/yyyy'
            });
        };
        $.fn.P2BTimeInlinePicker = function () {
            this.datetimepicker({
                lang: 'en',
                inline: true,
                datepicker: false,
                format: 'h:i A',
                formatTime: 'h:i A',
            });
        };


        $.fn.P2BDateTimePickertime = function (times) {
            if (typeof times === "undefined") {
                times = null;
            }
            this.datetimepicker({
                format: 'H:i',
                formatDate: 'dd/MM/yyyy',
                timeFormat: 'H:i',
                step: 1,
                allowTimes: times
            });
        }

        $.fn.P2BDateTimePicker = function (times) {
            if (typeof times === "undefined") {
                times = null;
            }
            this.datetimepicker({
                format: 'd/m/Y H:i',
                formatDate: 'dd/MM/yyyy',
                timeFormat: 'H:i',
                step: 1,
                allowTimes: times
            });
        }

        $.extend({
            P2BStartDateEndDateChecking: function (startdate, enddate) {
                if (Date.parse(enddate) < Date.parse(startdate)) {
                    value = "End Date Should be Greater Than Start Date";
                }
                else {
                    return "0";
                }
                return value;
            },
        });

        $.extend({
            distinct: function (anArray) {
                var result = [];
                $.each(anArray, function (i, v) {
                    if ($.inArray(v, result) == -1) result.push(v);
                });
                return result;
            }
        });
        $.fn.P2BGrid = function (ColNames, ColModel, SortName, Caption, url, width, height, pager, extraparam) {
            /*

            parm:extraparam.rowNum 
            type:int,
            desc:for how many record show on grid 
            
            parm:extraparam.multiple
            type:bool
            desc:multiple selection on grid

            parm:extraparam.selectall
            type:bool
            desc:show select all btn on pager,and select all record for db table,extraparam.multiple should be true

            */

            var ColNamesData = [];
            var ColModelData = [];
            for (var i = 0; i < ColNames; i++) {
                ColNamesData.push(ColNames[i]);
            }
            for (var j = 0; j < ColModel.length; j++) {
                ColModelData.push({ name: ColModel[j], index: ColModel[j], width: 430, align: "center" });
            }
            pager = pager || "#pager2";
            extraparam = extraparam || false;
            if (extraparam != undefined && extraparam.rowNum != undefined) {
                extraparam.rowNum = extraparam.rowNum || 10;
            }
            this.jqGrid({
                url: url,
                datatype: "json",
                colNames: ColNamesData,
                colModel: ColModelData,
                rowNum: extraparam.rowNum,
                rowList: [10, 20, 30],
                pager: pager,
                sortname: SortName,
                multiselect: extraparam.multiple,
                multiboxonly: extraparam.multiple,
                viewrecords: true,
                sortorder: "asc",
                caption: Caption,
                width: width,
                height: height,
                loadonce: true,
            });
            this.jqGrid('navGrid', pager, { edit: false, add: false, del: false });
            this.trigger('reloadGrid', [{ _search: false, searchField: null, searchOper: '', searchString: '' }]);

            this.jqGrid('filterToolbar', { stringResult: true, searchOnEnter: true, defaultSearch: 'cn' });
            if (extraparam.selectall == true && extraparam.multiple == true) {
                var init = $(this);
                this.jqGrid('navGrid', pager, { edit: false, add: false, del: false, search: false }).navButtonAdd(pager, {
                    caption: "SelectAll",
                    buttonicon: "ui-icon-arrow-2-n-s",
                    onClickButton: function () {
                        if (JqGridCheck.select_all == false) {
                            $.ajax({
                                url: url,
                                method: "POST",
                                success: function (data) {
                                    console.log(data);
                                    var a = [];
                                    $.each(data.rows, function (i, k) {
                                        a.push(k[0]);
                                    });
                                    if (a.length > 0) {
                                        alert("Total Selected Record : " + parseInt(a.length + 1) + "");
                                        $('#cb_' + $(init).attr('id') + '').trigger('click');
                                        JqGridCheck.select_all = true;
                                        JqGridCheck.Set(a);
                                    }
                                },
                                data: { rows: 0, page: 0, sord: "asc", filter: $('#txtPayMonth').val() }
                            });
                        } else {
                            $('#cb_' + $(init).attr('id') + '').trigger('click');
                            JqGridCheck.Del();
                        }
                    },
                    position: "last"
                });
            }
        };

        $.extend($.fn.P2BGrid, {
            P2BEdDelV: function (tablename, value) {
                var data;
                var id = jQuery(tablename).jqGrid('getGridParam', 'selrow');
                if (id) {
                    data = jQuery(tablename).jqGrid('getRowData', id, value);
                }
                else {
                    alert("Please Select Row");
                }
                return data;
            }
        });
        $.extend($.fn.P2BGrid, {
            onclickChangeUrl: function (gridname, url, data) {
                var postdata = $(gridname).jqGrid('getGridParam', 'postData');
                postdata._search = false;
                postdata.searchField = "";
                postdata.searchOper = "";
                postdata.searchString = "";
                $(gridname).setGridParam({ url: url, postData: { isAutho: data }, page: 1 }).trigger("reloadGrid");
            }
        });

        $.fn.P2BGridEditRow = function (ColNames, ColModel, SortName, Caption, url, tablename, eddelurl, columnvalue, width, height, forwarddata) {
            var editrow, table, celValue;
            var ColNamesData = [];
            var ColModelData = [];
            table = tablename;
            for (var i = 0; i < ColNames; i++) {
                ColNamesData.push(ColNames[i]);
            }
            for (var j = 0; j < ColModel.length; j++) {
                if (j == 0) ColModelData.push({ name: ColModel[j], index: ColModel[j], width: 150, align: "center", editable: false, editoptions: { size: 10 } });
                else ColModelData.push({ name: ColModel[j], index: ColModel[j], width: 150, align: "center", editable: true, editoptions: { size: 10 } });
            }
            this.jqGrid({
                url: url + '?extraeditdata=' + forwarddata + '',
                gridview: true,
                rownumbers: true,
                datatype: "json",
                colNames: ColNamesData,
                colModel: ColModelData,
                rowNum: 10,
                rowList: [10, 20, 30],
                pager: '#pager',
                sortname: SortName,
                viewrecords: true,
                sortorder: "asc",
                caption: Caption,
                search: {
                    odata: ['equal']
                },
                width: width,
                height: height,
                gridComplete: function () {
                    celValue = jQuery(this).jqGrid('getGridParam', 'selrow');
                    var id = jQuery(this).jqGrid().getDataIDs();
                    for (var i = 0; i < id.length; i++) {
                        editrow = id[i];
                        var html = "<div style='padding:0 0 0 37px;'><nav>" +
                            "<ul style='margin:0;'>" +
                            "<li style='display:block;float:left;margin:0;padding: 0 0.8em 0 0;position:relative;'><span class='ui-icon ui-icon-pencil' onclick=\"jQuery('" + table + "').P2BJqGridEdit('" + editrow + "','ed','" + eddelurl + "','" + columnvalue + "')\" onmouseover=\"jQuery(this).css('border','1px solid #18a689')\" onmouseout=\"jQuery(this).css('border','')\"; style='cursor:pointer' title='Edit Record'></span></li>" +
                            "<li style='display:block;float:left;margin:0;padding: 0 0.8em 0 0;position:relative;'><span class='ui-icon ui-icon-clipboard' style='cursor:pointer' onclick=\"jQuery('" + table + "').P2BJqGridEdit('" + editrow + "','view')\" onmouseover=\"jQuery(this).css('border','1px solid #18a689')\" onmouseout=\"jQuery(this).css('border','')\" title='View Record'></span></li>" +
                            "<li style='display:block;float:left;margin:0;padding: 0 0.8em 0 0;position:relative;'><span class='ui-icon ui-icon-trash'style='cursor:pointer' onclick=\"jQuery('" + table + "').P2BJqGridEdit('" + editrow + "','del','" + eddelurl + "','" + columnvalue + "')\"onmouseover=\"jQuery(this).css('border','1px solid #18a689')\" onmouseout=\"jQuery(this).css('border','')\" title='Delete Record'></span></li>" +
                            "</ul>" +
                            "</nav></div>";
                        jQuery(this).jqGrid('setRowData', id[i], { Actions: html });
                    }
                }
            });
            this.jqGrid('navGrid', '#pager', { edit: false, add: false, del: false });
            this.trigger('reloadGrid');
        };
        $.fn.P2BJqGridEdit = function (value, selectedButton, url, colname) {
            var celValue;
            celValue = jQuery(this).jqGrid('getCell', value, colname)
            if (selectedButton == "view") {
                if (value != null) jQuery(this).jqGrid('editGridRow', value, {
                    height: 200,
                    width: 200,
                    editCaption: 'View Record',
                    reloadAfterSubmit: false,
                    beforeShowForm: function ($form) {
                        $form.parent().find('#sData').hide()
                    }
                });
                else alert("Please Select Row");
                $('.FormElement').prop('disabled', true).css("background-color", "rgba(197, 196, 196, 0.59)");
            }
            else if (selectedButton == "del") {
                if (value != null) jQuery(this).jqGrid('delGridRow', value, {
                    reloadAfterSubmit: true,
                    beforeSubmit: false,
                    url: url + '?p2bparam=' + celValue + '',
                });
                else alert("Please Select Row to delete!");
            }
            else if (selectedButton == "ed") {
                if (value != null) jQuery(this).jqGrid('editGridRow', value, {
                    height: 200,
                    width: 200,
                    reloadAfterSubmit: true,
                    beforeSubmit: false,
                    url: url + '?p2bparam=' + celValue + '',
                });
                else alert("Please Select Row");
                $('.FormElement').removeClass().removeProp();
            }
        };
        $.widget("app.dialog", $.ui.dialog, {
            options: {
                iconButtons: []
            },
            _create: function () {
                this._super();
                var $titlebar = this.uiDialog.find(".ui-dialog-titlebar");
                $.each(this.options.iconButtons, function (i, v) {
                    var $button = $("<button/>").text(this.text),
                        right = $titlebar.find("[role='button']:last")
                            .css("right");
                    $button.button({ icons: { primary: this.icon }, text: false })
                        .addClass("ui-dialog-titlebar-close ui-dialog-titlebar-help")
                        .css("right", (parseInt(right) + 22) + "px")
                        .appendTo($titlebar);
                });
            }
        });
        function RemoveLookupTableElement(form) {
            var formname = $(form);
            console.log(formname);
            formname.find('table.lookuptableselected').find('tr td').parent().remove();
        }
        $.fn.P2BCreateDialog = function (creaturl, creadata, url, forwarddata, maindialogtitle, state, submiturl, submitnameformforserilize, savemessage, errormessage, gridreloadname, height, width, nameofthelookuptable, nameidclassofbuttontodisable, returnfunctiondata, fn) {
            jQuery(this).trigger('reset');
            var init = jQuery(this);
            var ajaxdata, createajaxdata;
            nameidclassofbuttontodisable = '.popup-content-icon-edit,.popup-content-icon-remove';
            var maindailog = jQuery(init).dialog({
                iconButtons: [{
                    text: "Help",
                    icon: "ui-icon-help",
                }],
                autoOpen: false,
                height: height,
                width: width,
                modal: true,
                closeOnEscape: false,
                title: maindialogtitle,
                beforeClose: function () {
                    RemoveLookupTableElement(submitnameformforserilize);
                    jQuery(init).find('select').empty().append("<option value=0 selected=true>-Select-</option>").selectmenu("refresh");
                    jQuery(init).find('input').empty();
                    jQuery(init).find('textarea').empty();
                    jQuery(init).find(nameidclassofbuttontodisable).button('enable').removeClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                    jQuery(init).find(nameofthelookuptable).find('tr td').parent().remove();
                },
                open: function (event, ui) {
                    $('.ui-dialog-titlebar-help').click(function () {
                        helpfun("create", "" + submitnameformforserilize.slice("4") + "");
                    });
                    createajaxdata = $.ajax({
                        url: creaturl,
                        method: 'POST',
                        data: { data: creadata }
                    });
                    createajaxdata.done(function (value) {
                        returnfunctiondata(value);
                    });
                    jQuery(init).find(nameidclassofbuttontodisable).button('disable').addClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                    OnpageAlter();
                    MakeRadioBtnChecked();
                },
                buttons: {
                    Submit: function () {
                        var x = PerformValidations(submitnameformforserilize);
                        var y = true;
                        if (fn != undefined) {
                            if (fn.validurl != null && x == true) {
                                var chkajx = $.ajax({
                                    url: fn.validurl,
                                    method: "POST",
                                    async: false,
                                    data: $(submitnameformforserilize).serialize(),
                                    beforeSend: function () {
                                        $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('disable').addClass('submitbtndisable');
                                        ajaxloaderv2('body');
                                    },
                                });
                                chkajx.success(function (msg) {
                                    $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('enable').removeClass('submitbtndisable');
                                    $('.ajax_loder').remove();
                                    if (msg.success == true) {
                                        //success event
                                        y = msg.success;
                                    } else {
                                        y = msg.success;
                                        var newDiv = $(document.createElement('div'));
                                        var htmltag = "";
                                        for (var i = 0; i < msg.responseText.length; i++) {
                                            htmltag += '<span style="float:left;display:block;margin:2px;"><span class="ui-icon ui-icon-alert" style="float:left;display:block"></span><span style="width:80%;"> ' + msg.responseText[i] + '</span></span>';
                                        }
                                        newDiv.html(htmltag);
                                        newDiv.dialog({
                                            autoOpen: false,
                                            title: "Validation",
                                            height: 250, width: 400, modal: true,
                                            buttons: {
                                                Ok: function (e) {
                                                    newDiv.dialog("close");
                                                    newDiv.remove();
                                                }
                                            }
                                        });
                                        newDiv.dialog('open');
                                        $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                                    }

                                });
                                chkajx.error(function (xhr, status, error) {
                                    $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('enable').removeClass('submitbtndisable');
                                    $('.ajax_loder').remove();
                                    y = false;
                                    var newDiv = $(document.createElement('div'));
                                    var htmltag = '<p><span class="ui-icon ui-icon-alert" style="float:left;margin-right:10px"></span> ' + xhr.status + '"-"' + xhr.statusText + '';
                                    htmltag += '</p>';
                                    newDiv.html(htmltag);
                                    newDiv.dialog({
                                        autoOpen: false,
                                        title: "Information",
                                        height: 130,
                                        width: 250,
                                        modal: true,
                                        buttons: {
                                            Ok: function () {
                                                newDiv.remove();
                                                newDiv.dialog("close");
                                                $(init).dialog("close");
                                            }
                                        }
                                    });
                                    newDiv.dialog('open');
                                    $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                                });
                            }
                        }
                        if (x == false || y == false) {
                            return false;
                        }
                        ajaxdata = $.ajax({
                            url: submiturl,
                            method: "POST",
                            data: $(submitnameformforserilize).serialize(),
                            beforeSend: function () {
                                $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('disable').addClass('submitbtndisable');
                                ajaxloaderv2('body');
                            },
                        });
                        ajaxdata.done(function (msg) {
                            $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('enable').removeClass('submitbtndisable');
                            $('.ajax_loder').remove();
                            var newDiv = $(document.createElement('div'));
                            var htmltag = '<p><span class="ui-icon ui-icon-check" style="float:left;margin-right:10px"></span> ' + savemessage + '' + msg[2] + '';
                            htmltag += '</p>';
                            newDiv.html(htmltag);
                            newDiv.dialog({
                                autoOpen: false,
                                title: "Information",
                                height: 150, width: 250, modal: true,
                                buttons: {
                                    Ok: function (e) {
                                        if (gridreloadname != '' || gridreloadname == null) {
                                            jQuery(gridreloadname).trigger('reloadGrid');
                                        }
                                        if (fn != undefined) {
                                            if (fn.datatablename != null) {
                                                var table = $(fn.datatablename).DataTable();
                                                table.ajax.reload();
                                            }
                                        }
                                        RemoveLookupTableElement(submitnameformforserilize);
                                        jQuery(init).find('select').empty().append("<option value=0 selected=true>-Select-</option>").selectmenu("refresh");
                                        jQuery(init).find('input').empty();
                                        jQuery(init).find('textarea').empty();
                                        jQuery(init).find(nameidclassofbuttontodisable).button('enable').removeClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                                        jQuery(init).find(nameofthelookuptable).find('tr td').parent().remove();
                                        newDiv.dialog("close");
                                        jQuery(maindailog).dialog("close");
                                    }
                                }
                            });
                            newDiv.dialog('open');
                            $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                        });
                        ajaxdata.fail(function (jqXHR, textStatus) {
                            $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('enable').removeClass('submitbtndisable');
                            $('.ajax_loder').remove();
                            var newDiv = $(document.createElement('div'));
                            var htmltag = '<p><span class="ui-icon ui-icon-alert" style="float:left;margin-right:10px"></span> ' + errormessage + '' + jqXHR.status + '"-"' + jqXHR.statusText + '';
                            htmltag += '</p>';
                            newDiv.html(htmltag);
                            newDiv.dialog({
                                autoOpen: false,
                                title: "Information",
                                height: 130,
                                width: 250,
                                modal: true,
                                buttons: {
                                    Ok: function () {
                                        if (gridreloadname != '' || gridreloadname == null) {
                                            jQuery(gridreloadname).trigger('reloadGrid');
                                        }
                                        if (fn != undefined) {
                                            if (fn.datatablename != null) {
                                                var table = $(fn.datatablename).DataTable();
                                                table.ajax.reload();
                                            }
                                        }
                                        newDiv.dialog("close");
                                    }
                                }
                            });
                            newDiv.dialog('open');
                            $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                        });
                    },
                    Cancel: function () {
                        jQuery(init).dialog("close");
                    }
                }
            });
            jQuery(init).dialog(state);
            $('.ui-dialog-buttonpane').find('button:contains("Submit")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-disk"></span>');
            $('.ui-dialog-buttonpane').find('button:contains("Cancel")').removeClass('ui-buttontext-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-closethick"></span>');
        };
        $.fn.P2BDeleteModalDialog = function (deleteurl, deletedata, deletemessage, deletesuccessmessage, deleteerrormessage, gridreloadname, height, width) {
            jQuery(this).trigger('reset');
            var deleteajaxdata;
            var newDiv = $(document.createElement('div'));
            var htmltag = '<p><span class="ui-icon ui-icon-alert" style="float:left;margin-right:10px"></span> ' + deletemessage + '';
            htmltag += '</p>';
            newDiv.html(htmltag);
            newDiv.dialog({
                autoOpen: false,
                title: "Delete Confirmation !",
                height: height,
                width: width,
                closeOnEscape: false,
                modal: true,
                buttons: {
                    Confirm: function () {
                        deleteajaxdata = $.ajax({
                            url: deleteurl,
                            method: 'POST',
                            data: { data: deletedata },
                            beforeSend: function () {
                                // alert('hiihihihihi');
                                $('.ui-dialog-buttonpane').find('button:contains("Confirm")').button('disable').addClass('submitbtndisable');
                                ajaxloaderv2('body');

                            },
                        });
                        deleteajaxdata.done(function (msg) {
                            $('.ui-dialog-buttonpane').find('button:contains("Confirm")').button('enable').removeClass('submitbtndisable');
                            $('.ajax_loder').remove();
                            var newDivdelete = $(document.createElement('div'));
                            var htmltag = '<p><span class="ui-icon ui-icon-check" style="float:left;margin-right:10px"></span> ' + deletesuccessmessage + '' + msg[1] + '';
                            htmltag += '</p>';
                            newDivdelete.html(htmltag);
                            newDivdelete.dialog({
                                autoOpen: false,
                                closeOnEscape: false,
                                title: "Information",
                                height: 185,
                                width: 425,
                                modal: true,
                                buttons: {
                                    Ok: function () {
                                        if (gridreloadname != '' || gridreloadname == null) {
                                            jQuery(gridreloadname).trigger('reloadGrid');
                                        }
                                        newDivdelete.dialog("close");
                                        jQuery(newDiv).dialog("close");
                                    }
                                }
                            });
                            newDivdelete.dialog('open');
                            $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                        });
                        deleteajaxdata.fail(function (jqXHR, textStatus) {
                            $('.ui-dialog-buttonpane').find('button:contains("Confirm")').button('enable').removeClass('submitbtndisable');
                            $('.ajax_loder').remove();
                            var newDivdelete = $(document.createElement('div'));
                            var htmltag = '<p><span class="ui-icon ui-icon-alert" style="float:left;margin-right:10px"></span> ' + deleteerrormessage + '" : "' + jqXHR.status + '"-"' + jqXHR.statusText + '';
                            htmltag += '</p>';
                            newDivdelete.html(htmltag);
                            newDivdelete.dialog({
                                autoOpen: false,
                                title: "Information",
                                closeOnEscape: false,
                                height: 'auto', width: 'auto', modal: true,
                                buttons: {
                                    Ok: function () {
                                        if (gridreloadname != '' || gridreloadname == null) {
                                            jQuery(gridreloadname).trigger('reloadGrid');
                                        }
                                        newDivdelete.dialog("close");
                                        jQuery(newDiv).dialog("close");
                                    }
                                }
                            });
                            newDivdelete.dialog('open');
                            $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                        });
                    },
                    Cancel: function () {
                        jQuery(newDiv).dialog("close");
                    }
                }
            });
            jQuery(newDiv).dialog('open');
            $('.ui-dialog-buttonpane').find('button:contains("Confirm")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-trash"></span>');
            $('.ui-dialog-buttonpane').find('button:contains("Cancel")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-closethick"></span>');
        };
        function get1DArray(arrtoconvert) {
            var new_arr = [];
            for (var i = 0; i < arrtoconvert.length; i++) {
                if (arrtoconvert[i] != "") {
                    new_arr = new_arr.concat(arrtoconvert[i]);
                }
            }
            return new_arr;
        };
        function CountTableIdsVal(lookuptable) {
            var arr = lookuptable.split(",");
            var abcd = [];
            var x;
            $.each(arr, function (i, k) {
                $('' + k + ' tr td ').each(function () {
                    x = $(this).find('input').val();
                    x = $(this).text();
                    if (x != "") {
                        abcd.push(x);
                    }
                });
            });
            return abcd;
        };
        $.fn.P2BEditModalDialog = function (openurl, opendataforward, editurl, maindialogtitle, forwardserializedata, forwarddata, editmessage, editerrormessage, gridreloadname, height, width, nameofthelookuptable, nameidclassofbuttontodisable, returndatafunction, fn) {
            var editajaxdata, editajaxopenloaddata, init;
            var OldIds = [];
            var NewIds = [];
            var olddata = [];
            nameidclassofbuttontodisable = '.popup-content-icon-edit,.popup-content-icon-remove';
            var confrm, flag;
            init = jQuery(this);
            var maindialog = jQuery(init).dialog({
                autoOpen: false,
                height: height,
                width: width,
                modal: true,
                iconButtons: [{
                    text: "Help",
                    icon: "ui-icon-help",
                    click: function (e) {
                        helpfun("edit", "" + submitnameformforserilize.slice("4") + "");
                    }
                }],
                closeOnEscape: false,
                title: maindialogtitle,
                beforeClose: function (e) {
                    RemoveLookupTableElement(forwardserializedata);
                    if (nameofthelookuptable != null && nameofthelookuptable != "") {
                        NewIds = CountTableIdsVal(nameofthelookuptable);
                    }
                    var olddata = get1DArray(OldIds);
                    console.log(NewIds);
                    console.log(olddata);
                    //bypass checking remaining
                    NewIds = [];
                    olddata = [];
                    if (NewIds.length == 0 && olddata.length == 0) {
                        flag = true;
                    }
                    $.each(olddata, function (i, k) {
                        if (NewIds.indexOf(k) > -1) {
                            flag = true;
                        } else {
                            flag = false;
                        }
                    });
                    if (flag == true && NewIds.length == olddata.length) {
                        NewIds = [];
                        olddata = [];
                        OldIds = [];
                        jQuery(init).find("select").empty().append("<option selected=true>-Select-</option>").removeClass("ui-selectmenu-text").selectmenu("refresh");
                        jQuery(init).find('input').empty().removeAttr('style').removeAttr('readonly');
                        jQuery(init).find('textarea').empty().removeAttr('style').removeAttr('readonly');
                        jQuery(init).find(nameidclassofbuttontodisable).button('enable').removeClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                        jQuery(init).find(nameofthelookuptable).find('tr td').parent().remove();
                        return true;
                    } else {
                        var confrm = confirm("Save Modified Data Or Else It Will Be Discarded");
                        //Ok-true
                        //cancel-false
                        if (confrm == false) {
                            return false;
                        } else {
                            NewIds = [];
                            olddata = [];
                            OldIds = [];
                            jQuery(init).find("select").empty().append("<option selected=true>-Select-</option>").removeClass("ui-selectmenu-text").selectmenu("refresh");
                            jQuery(init).find('input').empty().removeAttr('style').removeAttr('readonly');
                            jQuery(init).find('textarea').empty().removeAttr('style').removeAttr('readonly');
                            jQuery(init).find(nameidclassofbuttontodisable).button('enable').removeClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                            jQuery(init).find(nameofthelookuptable).find('tr td').parent().remove();
                            return true;
                        }
                    }

                    e.preventDefault();
                },
                open: function (event, ui) {
                    NewIds = [];
                    olddata = [];
                    OldIds = [];
                    editajaxopenloaddata = $.ajax({
                        url: openurl,
                        method: 'POST',
                        data: { data: opendataforward },
                        beforeSend: function () {
                            ajaxloaderv2('body');
                        },
                        complete: function () {
                            $('.ajax_loder').remove();
                        }
                    });
                    editajaxopenloaddata.done(function (value) {
                        if (value[1] != undefined || value[1] != null) {
                            if (value[1][0] != undefined || value[1][0] != null) {
                                $.each(value[1][0], function (i, k) {
                                    if (k != null) {
                                        OldIds.push(k);
                                    }
                                });
                            }
                        }
                        console.log(value);
                        if (typeof returndatafunction === 'function') {

                            returndatafunction(value);
                        } else {
                            console.log('returnfun is not define');
                        }
                    });
                    OnpageAlter();

                    jQuery(init).find(nameidclassofbuttontodisable).button('disable').addClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                },
                buttons: {
                    Submit: function (e) {
                        var x = PerformValidations(forwardserializedata);
                        var y = true;
                        if (fn != undefined) {
                            if (fn.validurl != null && x == true) {
                                var chkajx = $.ajax({
                                    url: fn.validurl,
                                    method: "POST",
                                    async: false,
                                    data: $(forwardserializedata).serialize(),
                                    beforeSend: function () {
                                        $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('disable').addClass('submitbtndisable');
                                        ajaxloaderv2('body');
                                    },
                                });
                                chkajx.success(function (msg) {
                                    $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('enable').removeClass('submitbtndisable');
                                    $('.ajax_loder').remove();
                                    if (msg.success == true) {
                                        //success event
                                        y = msg.success;
                                    } else {
                                        y = msg.success;
                                        var newDiv = $(document.createElement('div'));
                                        var htmltag = "";
                                        for (var i = 0; i < msg.responseText.length; i++) {
                                            htmltag += '<span style="float:left;display:block;margin:2px;"><span class="ui-icon ui-icon-alert" style="float:left;display:block"></span><span style="width:80%;"> ' + msg.responseText[i] + '</span></span>';
                                        }
                                        newDiv.html(htmltag);
                                        newDiv.dialog({
                                            autoOpen: false,
                                            title: "Validation",
                                            height: 250, width: 400, modal: true,
                                            buttons: {
                                                Ok: function (e) {
                                                    newDiv.dialog("close");
                                                    newDiv.remove();
                                                }
                                            }
                                        });
                                        newDiv.dialog('open');
                                        $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                                    }

                                });
                                chkajx.error(function (xhr, status, error) {
                                    $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('enable').removeClass('submitbtndisable');
                                    $('.ajax_loder').remove();
                                    y = false;
                                    var newDiv = $(document.createElement('div'));
                                    var htmltag = '<p><span class="ui-icon ui-icon-alert" style="float:left;margin-right:10px"></span> ' + xhr.status + '"-"' + xhr.statusText + '';
                                    htmltag += '</p>';
                                    newDiv.html(htmltag);
                                    newDiv.dialog({
                                        autoOpen: false,
                                        title: "Information",
                                        height: 130,
                                        width: 250,
                                        modal: true,
                                        buttons: {
                                            Ok: function () {
                                                newDiv.remove();
                                                newDiv.dialog("close");
                                                $(init).dialog("close");
                                            }
                                        }
                                    });
                                    newDiv.dialog('open');
                                    $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                                });
                            }
                        }
                        if (x == false || y == false) {
                            return false;
                        }
                        var newDiv2 = $(document.createElement('div'));
                        var htmltag = '<p><span class="ui-icon ui-icon-alert" style="float:left;margin-right:10px"></span> ' + 'Are You Sure Want To Edit Record???' + '';
                        htmltag += '</p>';
                        newDiv2.html(htmltag);
                        newDiv2.dialog({
                            autoOpen: false,
                            title: "Edit Confirmation !",
                            height: 170,
                            width: 305,
                            closeOnEscape: false,
                            modal: true,
                            buttons: {
                                Confirm: function () {
                                    editajaxdata = $.ajax({
                                        url: editurl,
                                        method: 'POST',
                                        data: $(forwardserializedata).serialize() + '&data=' + forwarddata + '',
                                        beforeSend: function () {
                                            // alert('hiihihihihi');
                                            $('.ui-dialog-buttonpane').find('button:contains("Confirm")').button('disable').addClass('submitbtndisable');
                                            ajaxloaderv2('body');

                                        },
                                    });
                                    editajaxdata.done(function (msg) {
                                        $('.ui-dialog-buttonpane').find('button:contains("Confirm")').button('enable').removeClass('submitbtndisable');
                                        $('.ajax_loder').remove();
                                        var newDiv = $(document.createElement('div'));
                                        var htmltag = '<p><span class="ui-icon ui-icon-check" style="float:left;margin-right:10px"></span> ' + editmessage + '' + msg[2] + '';
                                        htmltag += '</p>';
                                        newDiv.html(htmltag);
                                        newDiv.dialog({
                                            autoOpen: false,
                                            title: "Information",
                                            closeOnEscape: false,
                                            height: 150, width: 250, modal: true,
                                            buttons: {
                                                Ok: function (e) {
                                                    if (gridreloadname != '' || gridreloadname == null) {
                                                        jQuery(gridreloadname).trigger('reloadGrid');
                                                    }
                                                    NewIds = [];
                                                    olddata = [];
                                                    OldIds = [];
                                                    RemoveLookupTableElement(forwardserializedata);
                                                    jQuery(init).find('select').empty().append("<option value=0 selected=true>-Select-</option>").selectmenu("refresh");
                                                    jQuery(init).find('input').empty();
                                                    jQuery(init).find('textarea').empty();
                                                    jQuery(init).find(nameidclassofbuttontodisable).button('enable').removeClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                                                    jQuery(init).find(nameofthelookuptable).find('tr td').parent().remove();
                                                    newDiv.dialog("close");
                                                    jQuery(maindialog).dialog("close");
                                                    jQuery(newDiv2).dialog("close");

                                                }
                                            }
                                        });
                                        newDiv.dialog('open');
                                        $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                                    });
                                    editajaxdata.fail(function (jqXHR, textStatus) {
                                        $('.ui-dialog-buttonpane').find('button:contains("Confirm")').button('enable').removeClass('submitbtndisable');
                                        $('.ajax_loder').remove();
                                        var newDiv = $(document.createElement('div'));
                                        var htmltag = '<p><span class="ui-icon ui-icon-alert" style="float:left;margin-right:10px"></span> ' + editerrormessage + '' + jqXHR.status + '"-"' + jqXHR.statusText + '';
                                        htmltag += '</p>';
                                        newDiv.html(htmltag);
                                        newDiv.dialog({
                                            autoOpen: false,
                                            title: "Information", closeOnEscape: false,
                                            height: 130, width: 250, modal: true,
                                            buttons: {
                                                Ok: function () {
                                                    if (gridreloadname != '' || gridreloadname == null) {
                                                        jQuery(gridreloadname).trigger('reloadGrid');
                                                    }
                                                    newDiv.dialog("close");
                                                }
                                            }
                                        });
                                        newDiv.dialog('open');
                                        $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                                    });
                                },
                                Cancel: function () {
                                    jQuery(newDiv2).dialog("close");

                                }
                            }
                        });
                        jQuery(newDiv2).dialog('open');
                        $('.ui-dialog-buttonpane').find('button:contains("Confirm")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-trash"></span>');
                        $('.ui-dialog-buttonpane').find('button:contains("Cancel")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-closethick"></span>');

                    },
                    Cancel: function () {
                        jQuery(init).dialog('close');
                    }
                }
            });
            jQuery(init).dialog('open');
            $('.ui-dialog-buttonpane').find('button:contains("Submit")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-disk"></span>');
            $('.ui-dialog-buttonpane').find('button:contains("Cancel")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-closethick"></span>');
        };

        $.fn.P2BViewModalDialog = function (openurl, opendataforward, maindialogtitle, nameofthelookuptable, nameidclassofbuttontodisable, height, width, idorclassofautobtn, authoriseurl, autho_id, autho_action, autho_data, editmessage, editerrormessage, gridreloadname, returndatafunction) {
            var viewajaxopenloaddata;
            var old_data_class = ['.olddiv', '.oldlookup', '.olddrop', '.oldradio'];
            nameidclassofbuttontodisable = ".popup-content-icon-create,.popup-content-icon-edit,.popup-content-icon-remove,.popup-content-icon-lookup";
            var init = jQuery(this);
            autho_data = false;
            var maindialog = jQuery(init).dialog({
                autoOpen: false,
                height: height,
                width: width,
                modal: true,
                beforeClose: function () {
                    RemoveLookupTableElement(init);

                    jQuery(init).find("select").selectmenu('enable').empty().append("<option selected=true value=0>-Select-</option>").removeClass("ui-selectmenu-text").selectmenu("refresh");
                    jQuery(init).find('input').prop('disabled', false).removeAttr('style').removeAttr('readonly');
                    jQuery(init).find('textarea').prop('disabled', false).removeAttr('style').removeAttr('readonly');
                    jQuery(init).find('' + nameofthelookuptable + ' tr td').attr('disabled', 'disabled');
                    jQuery(init).find(nameofthelookuptable).find('tr td').parent().remove();
                    jQuery(init).find(nameidclassofbuttontodisable).button('enable').removeClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                    $.each(old_data_class, function (i, k) { $(init).find(k).remove(); });
                },
                closeOnEscape: false,
                title: maindialogtitle,
                open: function (event, ui) {
                    viewajaxopenloaddata = $.ajax({
                        url: openurl,
                        method: 'POST',
                        data: { data: opendataforward },
                        beforeSend: function () {
                            ajaxloaderv2('body');
                        },
                        complete: function () {
                            $('.ajax_loder').remove();
                        }
                    });
                    viewajaxopenloaddata.done(function (value) {
                        //$(init).find('').remove();
                        if (!$(idorclassofautobtn).hasClass("auto_active")) {
                            value[2] = null;
                            $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('disable');
                        } else {
                            autho_data = true;
                            $('.ui-dialog-buttonpane button:contains("Submit")').button("enable");
                        }
                        $.each(value, function (i, k) { console.log(k); });
                        returndatafunction(value);
                    });
                    jQuery(this).find("select").selectmenu('disable').addClass("ui-selectmenu-text");
                    jQuery(this).find('input').prop('disabled', true).css("background-color", "rgba(241, 241, 241, 0.66)");
                    jQuery(this).find('textarea').prop('disabled', true).css("background-color", "rgba(241, 241, 241, 0.66)");
                    jQuery(this).find('' + nameofthelookuptable + ' tr td').attr('disabled', 'disabled');
                    jQuery(this).find(nameidclassofbuttontodisable).button('disable').addClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                },
                buttons: {
                    Submit: function () {
                        var editajaxdata = $.ajax({
                            url: authoriseurl,
                            method: "POST",
                            data: {
                                auth_id: parseInt(autho_id), isauth: autho_data, auth_action: $(init).find("#autho_action").val(),
                            },
                            beforeSend: function () {
                                $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('disable').addClass('submitbtndisable');
                                ajaxloaderv2('body');

                            },
                        });
                        editajaxdata.done(function (msg) {
                            RemoveLookupTableElement(init);
                            $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('enable').removeClass('submitbtndisable');
                            $('.ajax_loder').remove();
                            var newDiv = $(document.createElement('div'));
                            var htmltag = '<p><span class="ui-icon ui-icon-check" style="float:left;margin-right:10px"></span> ' + editmessage + '' + msg[1] + '';
                            htmltag += '</p>';
                            newDiv.html(htmltag);
                            newDiv.dialog({
                                autoOpen: false,
                                title: "Information",
                                closeOnEscape: false,
                                height: 150, width: 250, modal: true,
                                buttons: {
                                    Ok: function () {
                                        if (gridreloadname != '' || gridreloadname == null) {
                                            jQuery(gridreloadname).trigger('reloadGrid');
                                        }
                                        jQuery(init).find("select").selectmenu('enable').empty().append("<option selected=true value=0>-Select-</option>").removeClass("ui-selectmenu-text").selectmenu("refresh");
                                        jQuery(init).find('input').prop('disabled', false).removeAttr('style').removeAttr('readonly');
                                        jQuery(init).find('textarea').prop('disabled', false).removeAttr('style').removeAttr('readonly');
                                        jQuery(init).find('' + nameofthelookuptable + ' tr td').attr('disabled', 'disabled');
                                        jQuery(init).find(nameofthelookuptable).find('tr td').parent().remove();
                                        jQuery(init).find(nameidclassofbuttontodisable).button('enable').removeClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                                        $.each(old_data_class, function (i, k) { $(init).find(k).remove(); });
                                        newDiv.dialog("close");
                                        newDiv.remove();
                                        jQuery(maindialog).dialog("close");
                                    }
                                }
                            });
                            newDiv.dialog('open');
                            $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                        });
                        editajaxdata.fail(function (jqXHR, textStatus) {
                            $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('enable').removeClass('submitbtndisable');
                            $('.ajax_loder').remove();
                            var newDiv = $(document.createElement('div'));
                            var htmltag = '<p><span class="ui-icon ui-icon-alert" style="float:left;margin-right:10px"></span> ' + editerrormessage + '' + jqXHR.status + '"-"' + jqXHR.statusText + '';
                            htmltag += '</p>';
                            newDiv.html(htmltag);
                            newDiv.dialog({
                                autoOpen: false,
                                title: "Information", closeOnEscape: false,
                                height: 130, width: 250, modal: true,
                                buttons: {
                                    Ok: function () {
                                        if (gridreloadname != '' || gridreloadname == null) {
                                            jQuery(gridreloadname).trigger('reloadGrid');
                                        }
                                        newDiv.dialog("close");
                                        newDiv.remove();
                                    }
                                }
                            });
                            newDiv.dialog('open');
                            $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                        });
                    },
                    Cancel: function () {
                        $(init).dialog("close");
                    }
                }
            });
            jQuery(init).dialog('open');
            $('.ui-dialog-buttonpane').find('button:contains("Submit")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-disk"></span>');
            $('.ui-dialog-buttonpane').find('button:contains("Cancel")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-closethick"></span>');
        };
        //---------------Utilities---Function----------------------------------------------------------------
        function lookuptablevaluecount(nameofloookuptable, btndisable) {
            //Count table row-lookuptable
            var x = $('' + nameofloookuptable + ' tr:has(td)').length;
            if (x == 0) {
                $(btndisable).button("disable");
            } else {
                $(btndisable).button("enable");
            }
        };
        function responseXmlData(url, responsetext) {
            //fire xml file request to database and response is in xml
            var xhttp = new XMLHttpRequest();
            xhttp.open("GET", url, true);
            xhttp.send();
            xhttp.onreadystatechange = function () {
                if (xhttp.readyState == 4 && xhttp.status == 200) {
                    return responsetext(xhttp.responseXML);
                }
            };
        };
        function helpfun(whitchtype, cameform) {
            var xDoc = responseXmlData("/xml/readxml", function (xmldata) {
                var xmldoc = $.parseXML(xmldata);
                console.log($(xmldoc));
            });
        }
        function responseXmlDataManipulation(xml) {
            return xml;
        };
        $.fn.onClickGrid = function (gridname, url1, url2) {
            $(this).on("click", function () {
                if ($(this).hasClass('auto_active')) {
                    console.log("click");
                    $(gridname).P2BGrid.onclickChangeUrl(gridname, url2, true);
                } else {
                    $(gridname).P2BGrid.onclickChangeUrl(gridname, url1, false);
                }
            });
        };
        $.fn.makeDisable = function (buttonsmakedisble) {
            var note_html = '<span class="parent_span" style="display:inline-block;position:relative"><span style="color:red">*</span><span>Note:The Text Which Are In<span class="child_span_color"></span>&nbsp;&nbsp;&nbsp;&nbsp; Color Is Old Data </span></span>';
            if (!$(this).hasClass('auto_active')) {
                $(buttonsmakedisble).button("enable");
            }
            $(this).on("click", function (e) {
                $(this).toggleClass('auto_active');
                if ($(this).hasClass('auto_active')) {
                    $('<div class="old_data_div">' + note_html + '</div>').appendTo('.ui-dialog-content');
                } else {
                    $('.ui-dialog-content').find('.old_data_div').remove();
                }
                if (!$(buttonsmakedisble).button("option", "disabled")) {
                    $(buttonsmakedisble).button("disable").css("background-color", "rgba(241, 241, 241, 0.66)");;
                } else {
                    $(buttonsmakedisble).button("enable").css("background-color", "");
                }
            });
        };
        function showajaxeroor(cameform, appendElement) {
            if (cameform.status == 404 || cameform.status == 500 || cameform.status == 0) {
                var htm = "<p style='position:absolute; top: 0%'><b>" + cameform.status + ":</b> Page " + cameform.statusText + "</p>";
                $(appendElement).append("" + htm + "");
            }
        };
        function ajaxloader(cameform) {
            $('<a><i class="fa fa-circle-o-notch fa-spin fa-5x fa-fw ajax_loder" style="color: rgba(27, 187, 173, 0.93)" aria-hidden="true"></i></a>').appendTo(cameform);
        };
        function ajaxloaderv2(cameform) {
            $('<a><i class="fa fa-circle-o-notch fa-spin fa-5x fa-fw ajax_loder" style="color: rgba(27, 187, 173, 0.93);z-index:999;" aria-hidden="true"></i></a>').appendTo(cameform);
        };
        var ajaxLoderRemove = function (init) {
            if (!init) {
                $('.ajax_loder').parents('div').remove();
            } else {
                $(init).find("div .ajax_loder").remove();
            }
            $(window).off('beforeunload');
        };
        $.fn.StickNote = function (old_val) {
            var objtype = $(this).attr('type') || $(this)[0].nodeName.toLowerCase();
            console.log(objtype);
            if (objtype == 'text') {
                $(this).oldVal(old_val);
            }
            if (objtype == 'radio') {
                $(this).oldRadioVal(old_val);
            }
            if (objtype == 'select') {
                $(this).oldDropval(old_val);
            }
            if (objtype == 'div') {
                $(this).oldLookupVal(old_val);
            }
        };
        $.fn.oldRadioVal = function (old_val) {
            $("<div class='oldradio'>" + old_val + "</div>").insertAfter(this);
        };
        $.fn.oldLookupVal = function (old_val) {
            $("<div class='oldlookup'>" + old_val + "</div>").appendTo(this);
        };
        $.fn.oldVal = function (old_val) {

            $("<div class='olddiv'>" + old_val + "</div>").insertAfter(this);
        };
        $.fn.oldDropval = function (old_val) {
            //to show old dropdown on page
            var loc = $(this).offset();
            var w = $(this).width();
            $("<div class='olddrop'>" + old_val + "</div>").insertAfter(this).css({ "margin-left": "" + w + "px" });
        };
        function OnpageAlter() {
            OnEnterFocusNext();
            ChangeInputWidth();
            AlterBtnType();
        }
        function MakeRadioBtnChecked() {
            $('input[type="radio"]').val([false]).button('refresh');
        };
        function OnEnterFocusNext() {
            //After Enter Btn On textbox move cursor to next textbox
            $('input[type="text"]').keyup(function (e) {
                if (e.keyCode == 13) {
                    $(this).parent().next().find('input[type="text"]').focus();
                }
            });
        };
        function ChangeInputWidth() {
            //$('.popup-content-textbox').css('min-width', $('.popup-content-textbox').css('width'));
        };
        function AlterBtnType() {
            //make all btn type to button
            $('button').attr('type', 'button');
        };
        var maxlength = function (control, len) {
            //console.log(len);
            if ($(control).val().length == 0 || $(control).val().length <= len) {
                return 'no-error';
            } else {
                $(control).data('maxlenght', len)
                ShowTextBoxErrorMsg('maxlength', control, "Max length Allowed is " + len + " ");
                return false;
            }
        };
        var AlphaNum = function (control) {
            $(control).val($(control).val().toUpperCase());
            var regx = /^[a-zA-Z0-9]+$/g;
            if ($(control).val().length != 0 && !(regx.test($(control).val()))) {
                ShowTextBoxErrorMsg('AlphaNum', control, "Enter Numbers or Alphabet..!");
                return false;
            } else {
                return 'no-error';
            }
        };
        var PersonName = function (control) {
            $(control).val($(control).val().toUpperCase());
            var regx = /^[a-zA-Z0-9-'_ ]+$/g;
            if ($(control).val().length != 0 && !(regx.test($(control).val()))) {
                ShowTextBoxErrorMsg('PersonName', control, "Enter Numbers or Alphabet..!");
                return false;
            } else {
                return 'no-error';
            }
        }
        var ShowTextBoxErrorMsg = function (comingfrom, control, msg) {
            $('div.error').text("");
            var pos = $(control).offset();
            var h = $(control).height();
            var w = $(control).width();
            $("<div class='error'>" + msg + "</div>").appendTo("body").css({ left: pos.left + w + 10, top: pos.top }).fadeOut(3000);
            if (comingfrom == 'AlphaNum') {
                $(control).val($(control).val().replace(/[^a-zA-Z0-9]/g, ''));
            } else if (comingfrom == 'maxlength') {
                var len = $(control).data('maxlenght');
                $(control).val($(control).val().substring(0, len));
            } else if (comingfrom == 'PersonName') {
                $(control).val($(control).val().replace(/[^a-zA-Z0-9-'_ ]/g, ''));
            }
        };
        $.fn.ValidateTextBox = function (fn) {
            var obj = $.extend({
                maxlength: 5,
                alphanum: null,
                name: null,
            }, fn);
            $(this).keyup(function (e) {
                var init = $(this);
                if (obj.maxlength != null) {
                    maxlength(init, obj.maxlength);
                }
                if (obj.alphanum != null) {
                    AlphaNum(init);
                }
                if (obj.name == true) {
                    PersonName(init);
                }
            });
        };

        ///Password Validation 
        var minlength = function (comingfrom, len) {
            var valid = true;
            var InValid = false;
            if ($(comingfrom).val().length < len && $(comingfrom).val().length != 0) {
                ShowErrorMsg(comingfrom, "Min length is " + len + "");
                return InValid;
            } else {
                return valid;
            }
        };
        var minupchars = function (comingfrom, count) {
            var valid = true;
            var InValid = false;
            if ($(comingfrom).val().replace(/[^A-Z]/g, "").length < count) {
                ShowErrorMsg(comingfrom, "Minimum " + count + " Uppercase Character Requred");
                return InValid;
            } else {
                return valid;
            }
        };
        var minlowchars = function (comingfrom, count) {
            var valid = true;
            var InValid = false;
            if ($(comingfrom).val().replace(/[^a-z]/g, "").length < count) {
                ShowErrorMsg(comingfrom, "Minimum " + count + " LowerCase Character Requred");
                return InValid;
            } else {
                return valid;
            }
        };
        var minnonos = function (comingfrom, count) {
            var valid = true;
            var InValid = false;
            if ($(comingfrom).val().replace(/\D/g, "").length < count) {
                ShowErrorMsg(comingfrom, "Minimum " + count + " Number Requred");
                return InValid;
            } else {
                return valid;
            }
        };
        var minsymbol = function (comingfrom, count) {
            var valid = true;
            var InValid = false;
            var CountSpecialChar = $(comingfrom).val().match(/[@#$%^&*`()_+\-=\~![\]{};':"\\|,.<>\/?]/g) || [];
            if (CountSpecialChar.length < count) {
                ShowErrorMsg(comingfrom, "Minimum " + count + " Special Requred");
                return InValid;
            } else {
                return valid;
            }
        };
        var ShowErrorMsg = function (comingfrom, msg) {
            console.log(msg);
            var pos = $(comingfrom).offset();
            var h = $(comingfrom).height();
            var w = $(comingfrom).width();
            if (!$('div.error')[0]) {
                $("<div class='error'>" + msg + "</div>")
                    .appendTo("body")
                    .css({
                        left: pos.left + 170,
                        top: pos.top + h - 10
                    });
            } else {
                return false;
            }
        };
        $.fn.DoValidation = function (comingfrom, fn) {
            var init = comingfrom;
            var msg = true;
            $.each(fn, function (i, k) {
                if (fn.hasOwnProperty(i)) {
                    if (msg == true && i == 'minlength') {
                        msg = minlength(init, fn.minlength);
                    }
                    if (msg == true && i == 'minupchars') {
                        msg = minupchars(init, fn.minupchars);
                    }
                    if (msg == true && i == 'minlowchars') {
                        msg = minlowchars(init, fn.minlowchars);
                    }
                    if (msg == true && i == "minnonos") {
                        msg = minnonos(init, fn.minnonos);
                    }
                    if (msg == true && i == "minsymbol") {
                        msg = minsymbol(init, fn.minsymbol);
                    }
                }
            });
            return msg;
        };
        $.fn.ValidatePassword = function (fn) {
            $(this).focusout(function (e) {
                if ($(this).val()) {
                    $(this).DoValidation(this, fn);
                }
            });
            $(this).keyup(function (e) {
                if ($('div.error')[0]) {
                    $('div.error').remove();
                }
            });
        };
        var MatchHeight = function (comingfrom) {
            $(window).on("resize", function () {
                var h = $(comingfrom).height();
                var w = $(comingfrom).width();
                $('div#door').height(h).width(w);
            });
        };
        var ValidateUser = function (comingfrom, url) {
            var hei = $(comingfrom).height();
            var wid = $(comingfrom).width();

            $(comingfrom).find('input').prop('disabled', true).css("background-color", "rgba(241, 241, 241, 0.66)");

            $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('disable');

            var htm =
                "<form id='pass_form'>" +
                "<div id='pass_text'>" +
                "<div style='float: left; width: 175px;'>" +
                "Enter the password to unlock this Form (Hit Enter) :</div> <input type='password' autofocus required id='pass2' />" +
                "</div></form>";
            $("<div id='door'>" + htm + "</div>").appendTo(comingfrom).addClass('pass_door').height(hei).width(wid);

            MatchHeight(comingfrom);

            $("div#door").slideDown("fast");

            $('#pass2').focus();

            $('#pass2').keydown(function (e) {
                //remove error icon
                $(".pass_error").remove();
            });

            $('#pass_form').submit(function (e) {

                var OnSuccess = function (data) {
                    if (data == "1") {
                        //error
                        $('div#door').effect("bounce", "fast");

                        $('<a class="pass_error">' +
                            '<i class="fa fa-times-circle-o fa-2x" style="position: absolute; margin-left: 5px; color: rgb(221, 22, 22);" aria-hidden="true">' +
                            '</i></a>').appendTo('#pass_text');
                    } else if (data == "0") {
                        //correct
                        $(comingfrom).find('input').prop('disabled', false).removeAttr('style').removeAttr('readonly');

                        $('.ui-dialog-buttonpane button:contains("Submit")').button("enable");

                        $(comingfrom).find('input').eq(0).focus();

                        $('div#door').slideUp("fast");
                    }
                };

                var OnError = function (data) {
                    alert(data);
                };

                $.ajax({
                    method: "Post",
                    url: url,
                    data: { data: $('#pass2').val() },
                    success: OnSuccess,
                    error: OnError
                });
                e.preventDefault();
            });

        };
        $.fn.P2BConfidentialModelDialog = function (fn) {
            var obj = $.extend({
                passurl: null,
                htmlurl: null,
                submiturl: null,
                form: null,
                height: null,
                width: null,
                title: null,
                Beforsendurl: null,
                returnfun: null,
                forwarddata: null
            }, fn);
            //passurl, url, maindialogtitle, state, submiturl, submitnameformforserilize, savemessage, errormessage, gridreloadname, height, width, forwarddata, ControlName, event, classoridoftheonwhichpopupderived, nameidclassofbuttontodisable, returnfunctiondata
            jQuery(this).trigger('reset');
            var ajaxdata, loadpartialajax, init = jQuery(this);
            var maindailog = jQuery(this).dialog({
                autoOpen: false,
                height: obj.height,
                width: obj.width,
                modal: true,
                closeOnEscape: false,
                title: obj.title,
                beforeClose: function () {
                    jQuery(init).find("select").detach();
                    // jQuery(classoridoftheonwhichpopupderived).find(nameidclassofbuttontodisable).button('enable').removeClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                    jQuery(init).remove();
                },
                open: function (event, ui) {
                    jQuery.ajax({
                        url: obj.htmlurl,
                        method: "GET",
                        beforeSend: function () {
                            ajaxloader(init);
                            $(init).find("div .ajax_loder").show();
                        },
                        success: function (result, status, xhr) {
                            showajaxeroor(xhr, init);
                            $(init).find("div .ajax_loder").hide();
                            $(init).html(result);
                            if (obj.BeforeSendurl != null) {

                                var ajaxloaddata = $.ajax({
                                    url: obj.BeforeSendurl,
                                    method: "POST",

                                });
                                ajaxloaddata.done(function (value) {
                                    if (typeof obj.returnfun === "function") {
                                        obj.returnfun(value);
                                    }
                                    //if (obj.mode.toUpperCase() == view) {
                                    //    $(init).find('input').attr('readonly', 'readonly');
                                    //}
                                });
                            }
                            if (obj.title != "DocumentUpload") {
                                ValidateUser(init, obj.passurl);
                            }
                            //OnEnterFocusNext();
                            AlterBtnType();
                            //   $(classoridoftheonwhichpopupderived).find(nameidclassofbuttontodisable).button('disable').button('refresh').addClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                        },
                        error: function (xhr, status, error) {
                            showajaxeroor(xhr, init);
                            $(init).find("div .ajax_loder").show();
                        },
                        complete: function () {
                            $(init).find("div .ajax_loder").hide();
                        }
                    });
                },
                buttons: {
                    Submit: function (e) {

                        var x = PerformValidations(obj.form);
                        //var a = $(obj.form).doval();
                        //console.log(a);
                        //if (x == true && a == true) {
                        if (x == true) {
                            if (obj.type == null) {
                                ajaxdata = $.ajax({
                                    url: obj.submiturl,
                                    method: "POST",
                                    data: $(obj.form).serialize() + '&forwarddata=' + obj.forwarddata + '',
                                    beforeSend: function () {
                                        // alert('hiihihihihi');
                                        $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('disable').addClass('submitbtndisable');
                                        ajaxloaderv2(init);

                                    },
                                });
                                ajaxdata.done(function (msg) {
                                    $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('enable').removeClass('submitbtndisable');
                                    $('.ajax_loder').remove();
                                    if (msg.success == true) {
                                        var newDiv = $(document.createElement('div'));
                                        var htmltag = '<p><span class="ui-icon ui-icon-check" style="float:left;margin-right:10px"></span>Message :' + msg.responseText + '';
                                        htmltag += '</p>';
                                        newDiv.html(htmltag);
                                        newDiv.dialog({
                                            autoOpen: false,
                                            title: "Information",
                                            closeOnEscape: false,
                                            height: 150, width: 250, modal: true,
                                            buttons: {
                                                Ok: function () {
                                                    obj.returnfun(msg.data);
                                                    newDiv.dialog("close");
                                                    $(newDiv).remove();
                                                    maindailog.dialog("close");
                                                    maindailog.remove();
                                                }
                                            }
                                        });
                                    } else {

                                        var newDiv = $(document.createElement('div'));
                                        var htmltag = '<p><span class="ui-icon ui-icon-alert" style="float:left;margin-right:10px"></span>Message :' + msg.responseText + '';
                                        htmltag += '</p>';
                                        newDiv.html(htmltag);
                                        newDiv.dialog({
                                            autoOpen: false,
                                            title: "Information",
                                            closeOnEscape: false,
                                            height: 150, width: 250, modal: true,
                                            buttons: {
                                                Ok: function () {
                                                    obj.returnfun(msg);
                                                    newDiv.dialog("close");
                                                    $(newDiv).remove();
                                                    // maindailog.dialog("close");
                                                    // maindailog.remove();
                                                }
                                            }
                                        });
                                    }
                                    newDiv.dialog('open');
                                    $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                                });
                                ajaxdata.fail(function (jqXHR, textStatus) {
                                    $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('enable').removeClass('submitbtndisable');
                                    $('.ajax_loder').remove();
                                    var newDiv = $(document.createElement('div'));
                                    var htmltag = '<p><span class="ui-icon ui-icon-alert" style="float:left;margin-right:10px"></span>Meassage :' + + '' + jqXHR.status + '"-"' + jqXHR.statusText + '';
                                    htmltag += '</p>';
                                    newDiv.html(htmltag);
                                    newDiv.dialog({
                                        autoOpen: false,
                                        title: "Information", closeOnEscape: false,
                                        height: 130, width: 250, modal: true,
                                        buttons: {
                                            Ok: function () {
                                                newDiv.dialog("close");
                                                newDiv.remove();
                                                $(maindailog).dialog('close');
                                                $(maindailog).remove();
                                            }
                                        }
                                    });
                                    newDiv.dialog('open');
                                    $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                                });
                            }
                            else if (obj.type.toUpperCase() == "FILE") {

                                var tempExcel = new FormData($(obj.form)[0]);
                                ajaxdata = $.ajax({
                                    url: obj.submiturl,
                                    method: "POST",
                                    data: tempExcel,
                                    cache: false,
                                    timeout: 600000,
                                    datatype: "json",
                                    enctype: 'multipart/form-data',
                                    processData: false,
                                    contentType: false,
                                    beforeSend: function () {
                                        // alert('hiihihihihi');
                                        $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('disable').addClass('submitbtndisable');
                                        ajaxloaderv2();

                                    },
                                });
                                ajaxdata.success(function (msg) {
                                    $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('enable').removeClass('submitbtndisable');
                                    //$('.ajax_loder').parents('div').remove();
                                    ajaxLoderRemove();
                                    if (msg.success == true) {
                                        var newDiv = $(document.createElement('div'));
                                        var htmltag = '<p><span class="ui-icon ui-icon-check" style="float:left;margin-right:10px"></span> ' + msg.responseText + '';
                                        htmltag += '</p>';
                                        newDiv.html(htmltag);
                                        newDiv.dialog({
                                            autoOpen: false,
                                            title: "Information",
                                            height: 150, width: 250, modal: true,
                                            buttons: {
                                                Ok: function (e) {
                                                    jQuery(init).dialog("close");
                                                    $(init).remove();
                                                    newDiv.dialog("close");
                                                    newDiv.remove();
                                                    jQuery(dia).dialog("close");
                                                    obj.onsubmitreturnfunction(msg);
                                                }
                                            }
                                        });
                                        newDiv.dialog('open');
                                        $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                                    } else {
                                        var newDiv = $(document.createElement('div'));
                                        var htmltag = '<p><span class="ui-icon ui-icon-check" style="float:left;margin-right:10px"></span> ' + msg.responseText + '';
                                        htmltag += '</p>';
                                        newDiv.html(htmltag);
                                        newDiv.dialog({
                                            autoOpen: false,
                                            title: "Information",
                                            height: 150, width: 250, modal: true,
                                            buttons: {
                                                Ok: function (e) {
                                                    newDiv.dialog("close");
                                                    newDiv.remove();
                                                }
                                            }
                                        });
                                        newDiv.dialog('open');
                                        $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                                    }
                                });
                                ajaxdata.error(function (xhr, status, error) {
                                    $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('enable').removeClass('submitbtndisable');
                                    //$('.ajax_loder').parents('div').remove();
                                    ajaxLoderRemove();
                                    var newDiv = $(document.createElement('div'));
                                    var htmltag = '<p><span class="ui-icon ui-icon-alert" style="float:left;margin-right:10px"></span> ' + xhr.status + '"-"' + xhr.statusText + '';
                                    htmltag += '</p>';
                                    newDiv.html(htmltag);
                                    newDiv.dialog({
                                        autoOpen: false,
                                        title: "Information",
                                        height: 130,
                                        width: 250,
                                        modal: true,
                                        buttons: {
                                            Ok: function () {
                                                newDiv.remove();
                                                newDiv.dialog("close");
                                                jQuery(dia).dialog("close");
                                            }
                                        }
                                    });
                                    newDiv.dialog('open');
                                    $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                                });
                            }
                        }
                    },
                    Cancel: function () {
                        jQuery(init).dialog("close");
                        $(init).remove();
                    }
                }
            });
            jQuery(init).dialog("open");
            $('.ui-dialog-buttonpane').find('button:contains("Submit")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-disk"></span>');
            $('.ui-dialog-buttonpane').find('button:contains("Cancel")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-closethick"></span>');
        };
        function CountTableIds(lookuptable) {
            var abcd = [];
            var x;
            $('' + lookuptable + ' tr td ').each(function () {
                x = $(this).find('input').val();
                if (x != "" && x != undefined) {
                    abcd.push(parseInt(x));
                }
            });
            return abcd;
        };
        function CountTranscationTableIds(lookuptable) {
            var abcd = [];
            var x;
            //console.log($('#' + lookuptable + ' tr td'));
            $('#' + lookuptable + ' tr td input').each(function () {
                x = $(this).val();
                if (x != "" && x != undefined) {
                    abcd.push(parseInt(x));
                }
            });
            return abcd;
        };
        $.LocalStorageHelper = {
            Length: function () {
                return localStorage.length;
            },
            Check: function () {
                if (typeof (Storage) == "undefined") {
                    return false;
                } else {
                    console.log("Web Storage Available");
                }
            },
            Set: function (arg1, arg2) {
                //console.log(arg2);
                if (typeof arg2 == "object") {
                    console.log(typeof arg2);
                    console.log(JSON.stringify(arg2));
                    localStorage.setItem(arg1, JSON.stringify(arg2));
                }
                localStorage.setItem(arg1, arg2);
            },
            Get: function (arg) {
                return localStorage.getItem(arg);
            },
            Del: function () {
                localStorage.clear();
            },
            RemoveAt: function (arg) {
                localStorage.removeItem(arg);
            }
        };
        $.fn.P2BPartialCreateModalDialog = function (url, maindialogtitle, state, submiturl, submitnameformforserilize, savemessage, errormessage, gridreloadname, height, width, forwarddata, ControlName, event, classoridoftheonwhichpopupderived, nameidclassofbuttontodisable, returnfunctiondata, fn) {
            jQuery(this).trigger('reset');
            var ajaxdata, loadpartialajax, init;
            init = jQuery(this);
            nameidclassofbuttontodisable = '.popup-content-icon-edit,.popup-content-icon-remove';
            var maindailog = jQuery(this).dialog({
                autoOpen: false,
                height: height,
                width: width,
                modal: true,
                closeOnEscape: false,
                title: maindialogtitle,
                beforeClose: function () {
                    jQuery(init).find("select").detach();
                    jQuery(submitnameformforserilize).find(nameidclassofbuttontodisable).button('enable').removeClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                    jQuery(init).remove();
                },
                open: function (event, ui) {
                    jQuery.ajax({
                        url: url,
                        method: "GET",
                        beforeSend: function () {
                            ajaxloader(init);
                            $(init).find("div .ajax_loder").show();
                        },
                        success: function (result, status, xhr) {
                            showajaxeroor(xhr, init);
                            $(init).find("div .ajax_loder").hide();
                            $(init).html(result);
                            MakeRadioBtnChecked();
                            OnEnterFocusNext();
                            AlterBtnType();
                            $(submitnameformforserilize).find(nameidclassofbuttontodisable).button('disable').button('refresh').addClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                        },
                        error: function (xhr, status, error) {
                            showajaxeroor(xhr, init);
                            $(init).find("div .ajax_loder").show();
                        },
                        complete: function () {
                            $(init).find("div .ajax_loder").hide();
                        }
                    });
                },
                buttons: {
                    Submit: function () {
                        var x = PerformValidations(submitnameformforserilize);
                        var y = true;
                        if (fn != undefined) {
                            if (fn.validurl != null && x == true) {
                                var chkajx = $.ajax({
                                    url: fn.validurl,
                                    method: "POST",
                                    async: false,
                                    data: $(submitnameformforserilize).serialize(),
                                    beforeSend: function () {
                                        $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('disable').addClass('submitbtndisable');
                                        ajaxloaderv2('body');
                                    },
                                });
                                chkajx.success(function (msg) {
                                    $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('enable').removeClass('submitbtndisable');
                                    $('.ajax_loder').remove();
                                    if (msg.success == true) {
                                        //success event
                                        y = msg.success;
                                    } else {
                                        y = msg.success;
                                        var newDiv = $(document.createElement('div'));
                                        var htmltag = "";
                                        for (var i = 0; i < msg.responseText.length; i++) {
                                            htmltag += '<span style="float:left;display:block;margin:2px;"><span class="ui-icon ui-icon-alert" style="float:left;display:block"></span><span style="width:80%;"> ' + msg.responseText[i] + '</span></span>';
                                        }
                                        newDiv.html(htmltag);
                                        newDiv.dialog({
                                            autoOpen: false,
                                            title: "Validation",
                                            height: 250, width: 400, modal: true,
                                            buttons: {
                                                Ok: function (e) {
                                                    newDiv.dialog("close");
                                                    newDiv.remove();
                                                }
                                            }
                                        });
                                        newDiv.dialog('open');
                                        $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                                    }

                                });
                                chkajx.error(function (xhr, status, error) {
                                    $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('enable').removeClass('submitbtndisable');
                                    $('.ajax_loder').remove();
                                    y = false;
                                    var newDiv = $(document.createElement('div'));
                                    var htmltag = '<p><span class="ui-icon ui-icon-alert" style="float:left;margin-right:10px"></span> ' + xhr.status + '"-"' + xhr.statusText + '';
                                    htmltag += '</p>';
                                    newDiv.html(htmltag);
                                    newDiv.dialog({
                                        autoOpen: false,
                                        title: "Information",
                                        height: 130,
                                        width: 250,
                                        modal: true,
                                        buttons: {
                                            Ok: function () {
                                                newDiv.remove();
                                                newDiv.dialog("close");
                                                $(init).dialog("close");
                                            }
                                        }
                                    });
                                    newDiv.dialog('open');
                                    $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                                });
                            }
                        }
                        if (x == false || y == false) {
                            return false;
                        }
                        ajaxdata = $.ajax({
                            url: submiturl,
                            method: "POST",
                            beforeSend: function () {
                                $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('disable').addClass('submitbtndisable');
                                ajaxloaderv2('body');
                            },
                            data: $(submitnameformforserilize).serialize() + '&data=' + forwarddata + '',
                        });
                        ajaxdata.done(function (msg) {
                            $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('enable').removeClass('submitbtndisable');
                            $('.ajax_loder').remove();
                            var newDiv = $(document.createElement('div'));
                            var htmltag = '<p><span class="ui-icon ui-icon-check" style="float:left;margin-right:10px"></span> ' + savemessage + '' + msg[2] + '';
                            htmltag += '</p>';
                            newDiv.html(htmltag);
                            newDiv.dialog({
                                autoOpen: false,
                                title: "Information",
                                closeOnEscape: false,
                                height: 150, width: 250, modal: true,
                                buttons: {
                                    Ok: function () {
                                        returnfunctiondata(msg);
                                        if (gridreloadname != '' || gridreloadname != null) {
                                            //jQuery(gridreloadname).trigger('reloadGrid');
                                        }
                                        if (ControlName != '' || ControlName != null) {
                                            jQuery(ControlName).trigger(event);
                                        }
                                        newDiv.dialog("close");
                                        newDiv.remove();
                                        jQuery(maindailog).dialog("close");
                                        jQuery(init).remove();
                                    }
                                }
                            });
                            newDiv.dialog('open');
                            $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                        });
                        ajaxdata.fail(function (jqXHR, textStatus) {
                            $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('enable').removeClass('submitbtndisable');
                            $('.ajax_loder').remove();
                            var newDiv = $(document.createElement('div'));
                            var htmltag = '<p><span class="ui-icon ui-icon-alert" style="float:left;margin-right:10px"></span> ' + errormessage + '' + jqXHR.status + '"-"' + jqXHR.statusText + '';
                            htmltag += '</p>';
                            newDiv.html(htmltag);
                            newDiv.dialog({
                                autoOpen: false,
                                title: "Information", closeOnEscape: false,
                                height: 130, width: 250, modal: true,
                                buttons: {
                                    Ok: function () {
                                        newDiv.dialog("close");
                                    }
                                }
                            });
                            newDiv.dialog('open');
                            $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                        });
                    },
                    Cancel: function () {
                        jQuery(init).dialog("close");
                    }
                }
            });
            jQuery(this).dialog(state);
            $('.ui-dialog-buttonpane').find('button:contains("Submit")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-disk"></span>');
            $('.ui-dialog-buttonpane').find('button:contains("Cancel")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-closethick"></span>');
        };
        $.fn.P2BPatialEditModalDialog = function (url, openurl, opendataforward, editurl, maindialogtitle, forwardserializedata, forwarddata, editmessage, editerrormessage, gridreloadname, height, width, classoridoftheonwhichpopupderived, nameclassidofinlinelookup, nameofthelist_inlinelookuptable, nameoftable_inlinelookuptable, multiple_allowed_or_not, nameidclassofbuttontodisable, returndatafunction, fn) {
            if (typeof opendataforward == 'undefined' || opendataforward == null) {
                $('<div></div>').P2BMessageModalDialog('ui-icon-alert', "Please Select Row..!");
                return false;
            };
            var editajaxdata, editajaxopenloaddata, init;
            init = jQuery(this);
            var maindialog = jQuery(this).dialog({
                autoOpen: false,
                height: height,
                width: width,
                modal: true,
                closeOnEscape: false,
                beforeClose: function () {
                    jQuery(forwardserializedata).find(nameidclassofbuttontodisable).button('enable').removeClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)").button('refresh');
                    jQuery(init).remove();
                },
                title: maindialogtitle,
                open: function (event, ui) {
                    function assigndata() {
                        editajaxopenloaddata = $.ajax({
                            url: openurl,
                            method: 'POST',
                            data: { data: opendataforward },
                        });
                        editajaxopenloaddata.done(function (value) {
                            returndatafunction(value);
                        });
                    };
                    jQuery.ajax({
                        url: url,
                        method: "GET",
                        beforeSend: function () {
                            ajaxloader(init);
                            $(init).find("div .ajax_loder").show();
                        },

                        success: function (result, status, xhr) {
                            showajaxeroor(xhr, init);
                            $(init).find("div .ajax_loder").hide();
                            $(init).html(result);
                            assigndata();
                            OnEnterFocusNext();
                            AlterBtnType();
                            console.log(forwarddata);
                            console.log(nameclassidofinlinelookup);
                            $(forwardserializedata).find(nameidclassofbuttontodisable).button('disable').button('refresh').addClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                        },
                        error: function (xhr, status, error) {
                            showajaxeroor(xhr, init);
                            $(init).find("div .ajax_loder").show();
                        },
                        complete: function () {
                            $(init).find("div .ajax_loder").hide();
                        }
                    });
                },
                buttons: {
                    Submit: function () {
                        var x = PerformValidations(forwardserializedata);
                        var y = true;
                        if (fn != undefined) {
                            if (fn.validurl != null && x == true) {
                                var chkajx = $.ajax({
                                    url: fn.validurl,
                                    method: "POST",
                                    async: false,
                                    data: $(forwardserializedata).serialize(),
                                    beforeSend: function () {
                                        $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('disable').addClass('submitbtndisable');
                                        ajaxloaderv2('body');
                                    },
                                });
                                chkajx.success(function (msg) {
                                    debugger;
                                    $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('enable').removeClass('submitbtndisable');
                                    $('.ajax_loder').remove();
                                    if (msg.success == true) {
                                        //success event
                                        y = msg.success;
                                    } else {
                                        y = msg.success;
                                        var newDiv = $(document.createElement('div'));
                                        var htmltag = "";
                                        for (var i = 0; i < msg.responseText.length; i++) {
                                            htmltag += '<span style="float:left;display:block;margin:2px;"><span class="ui-icon ui-icon-alert" style="float:left;display:block"></span><span style="width:80%;"> ' + msg.responseText[i] + '</span></span>';
                                        }
                                        newDiv.html(htmltag);
                                        newDiv.dialog({
                                            autoOpen: false,
                                            title: "Information",
                                            height: 150, width: 250, modal: true,
                                            buttons: {
                                                Ok: function (e) {
                                                    newDiv.dialog("close");
                                                    newDiv.remove();
                                                }
                                            }
                                        });
                                        newDiv.dialog('open');
                                        $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                                    }

                                });
                                chkajx.error(function (xhr, status, error) {
                                    $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('enable').removeClass('submitbtndisable');
                                    $('.ajax_loder').remove();
                                    y = false;
                                    var newDiv = $(document.createElement('div'));
                                    var htmltag = '<p><span class="ui-icon ui-icon-alert" style="float:left;margin-right:10px"></span> ' + xhr.status + '"-"' + xhr.statusText + '';
                                    htmltag += '</p>';
                                    newDiv.html(htmltag);
                                    newDiv.dialog({
                                        autoOpen: false,
                                        title: "Information",
                                        height: 130,
                                        width: 250,
                                        modal: true,
                                        buttons: {
                                            Ok: function () {
                                                newDiv.remove();
                                                newDiv.dialog("close");
                                                $(init).dialog("close");
                                            }
                                        }
                                    });
                                    newDiv.dialog('open');
                                    $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                                });
                            }
                        }
                        if (x == false || y == false) {
                            return false;
                        }

                        editajaxdata = $.ajax({
                            url: editurl,
                            method: "POST",
                            beforeSend: function () {
                                $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('disable').addClass('submitbtndisable');
                                ajaxloaderv2('body');
                            },
                            data: $(forwardserializedata).serialize() + '&data=' + forwarddata + '',
                        });
                        editajaxdata.done(function (msg) {
                            debugger;
                            $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('enable').removeClass('submitbtndisable');
                            $('.ajax_loder').remove();
                            var htmltag = "";
                            var newDiv = $(document.createElement('div'));
                            for (var i = 0; i < msg.responseText.length; i++) {
                                htmltag += '<span class="ajax-action-class-container"><span style="float:left;display:block"><i class="fa fa-fw fa-3x fa-check-circle-o ajax-success-icon" aria-hidden="true"></i></span><span class="ajax-action-text"> ' + msg.responseText[i] + '</span></span>';
                            }
                            //var htmltag = '<p><span class="ui-icon ui-icon-check" style="float:left;margin-right:10px"></span> ' + editmessage + '' + msg[2] + '';
                            //htmltag += '</p>';
                            var data = [];
                            data.push(msg.Id);
                            data.push(msg.Val);
                            newDiv.html(htmltag);
                            newDiv.dialog({
                                autoOpen: false,
                                title: "Information", closeOnEscape: false,
                                height: 150, width: 250,
                                modal: true,
                                buttons: {
                                    Ok: function () {
                                        if (gridreloadname != '' || gridreloadname == null) {
                                            //jQuery(gridreloadname).trigger('reloadGrid');
                                        }
                                        else {
                                            alert("Parameter is Not Passed Properly");
                                        }
                                        if (nameclassidofinlinelookup != '' || nameclassidofinlinelookup == null && classoridoftheonwhichpopupderived != '' || classoridoftheonwhichpopupderived == null) {
                                            //console.log(classoridoftheonwhichpopupderived);
                                            //console.log(nameclassidofinlinelookup);
                                            jQuery(classoridoftheonwhichpopupderived).find('' + nameclassidofinlinelookup + ' tr td:contains(' + data[0] + ')').parent('tr').remove();
                                            $(nameclassidofinlinelookup).P2BLookUpEncapsulate(nameclassidofinlinelookup, nameofthelist_inlinelookuptable, data[0], data[1], nameoftable_inlinelookuptable, nameidclassofbuttontodisable, multiple_allowed_or_not);
                                        }
                                        else {
                                            alert("Parameter is Not Passed Properly.");
                                        }
                                        newDiv.dialog("close");
                                        newDiv.remove();
                                        jQuery(forwardserializedata).find(nameidclassofbuttontodisable).button('enable').removeClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                                        jQuery(maindialog).dialog("close");
                                        jQuery(init).remove();
                                    }
                                }
                            });
                            newDiv.dialog('open');
                            $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                        });
                        editajaxdata.fail(function (jqXHR, textStatus) {
                            $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('enable').removeClass('submitbtndisable');
                            $('.ajax_loder').remove();
                            var newDiv = $(document.createElement('div'));
                            var htmltag = '<p><span class="ui-icon ui-icon-alert" style="float:left;margin-right:10px"></span> ' + editerrormessage + '' + jqXHR.status + '"-"' + jqXHR.statusText + '';
                            htmltag += '</p>';
                            newDiv.html(htmltag);
                            newDiv.dialog({
                                autoOpen: false,
                                title: "Information",
                                height: 130, width: 250, modal: true,
                                buttons: {
                                    Ok: function () {
                                        if (gridreloadname != '' || gridreloadname == null) {
                                            //	jQuery(gridreloadname).trigger('reloadGrid');
                                        }
                                        newDiv.dialog("close");
                                        jQuery(forwardserializedata).find(nameidclassofbuttontodisable).button('enable').removeClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                                    }
                                }
                            });
                            newDiv.dialog('open');
                            $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                        });

                    },
                    Cancel: function () {
                        jQuery(init).dialog('close');
                    }
                }
            });
            jQuery(init).dialog('open');
            $('.ui-dialog-buttonpane').find('button:contains("Submit")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-disk"></span>');
            $('.ui-dialog-buttonpane').find('button:contains("Cancel")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-closethick"></span>');
        };
        $.fn.P2BPartialDeleteModalDialog = function (deleteurl, deletedata, forwarddata, deletemessage, deletesuccessmessage, deleteerrormessage, selectfield, optionvalue, height, width, classoridoftheonwhichpopupderived, nameclassidofinlinelookup, btndisable) {
            if (typeof deletedata == 'undefined' || deletedata == null) {
                $('<div></div>').P2BMessageModalDialog('ui-icon-alert', "Please Select Row..!");
                return false;
            };
            var deleteajaxdata;
            deletedata, forwarddata = 0;
            var newDiv = $(document.createElement('div'));
            var htmltag = '<p><span class="ui-icon ui-icon-alert" style="float:left;margin-right:10px"></span> ' + deletemessage + '';
            htmltag += '</p>';
            newDiv.html(htmltag);
            newDiv.dialog({
                autoOpen: false,
                title: "Delete Confirmation !",
                height: height,
                width: width,
                modal: true,
                beforeClose: function () {
                    newDiv.remove();
                },
                buttons: {
                    Confirm: function () {
                        var newDivdelete = $(document.createElement('div'));
                        var htmltag = '<p><span class="ui-icon ui-icon-check" style="float:left;margin-right:10px"></span> ' + deletesuccessmessage + '' + "Record Removed" + '';
                        htmltag += '</p>';
                        newDivdelete.html(htmltag);
                        newDivdelete.dialog({
                            closeOnEscape: false,
                            dialogClass: "no-close",
                            autoOpen: false,
                            title: "Information",
                            height: "auto",
                            width: 425,
                            modal: true,
                            buttons: {
                                Ok: function (e) {
                                    if (selectfield != '' || deletedata != '' || selectfield != null || deletedata != null) {
                                        jQuery(selectfield + ' option[value="' + deletedata + '"]').remove();
                                        newDivdelete.dialog("close");
                                        newDivdelete.remove();
                                        jQuery(newDiv).dialog("close");
                                        jQuery(newDiv).remove();
                                    }
                                    else {
                                        alert("Option Value and Select Field is Not Defined");
                                    }
                                    if (classoridoftheonwhichpopupderived != '' || classoridoftheonwhichpopupderived == null && nameclassidofinlinelookup != '' || nameclassidofinlinelookup == null) {
                                        jQuery(classoridoftheonwhichpopupderived).find('' + nameclassidofinlinelookup).find("tr:has(td:eq('0'):contains('" + deletedata + "'))").remove();
                                    }
                                    var nameidclassofbuttontodisable = ".popup-content-icon-edit,.popup-content-icon-remove";
                                    var a = $(nameclassidofinlinelookup).parents('div').next('.icon-row').find(nameidclassofbuttontodisable);
                                    console.log(a);
                                    btndisable = a;
                                    lookuptablevaluecount(nameclassidofinlinelookup, btndisable);
                                }
                            }
                        });
                        newDivdelete.dialog('open');
                        $('.ui-dialog-buttonpane').find('button:contains("Remove")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-closethick"></span>');
                        $('.ui-dialog-buttonpane').find('button:contains("Cancel")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-cancel"></span>');
                    },
                    Cancel: function () {
                        jQuery(newDiv).dialog("close");
                    }
                }
            });
            jQuery(newDiv).dialog('open');
            $('.ui-dialog-buttonpane').find('button:contains("Confirm")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-trash"></span>');
            $('.ui-dialog-buttonpane').find('button:contains("Cancel")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-closethick"></span>');
        };
        $.fn.P2BLookUpModal = function (lookupurl, lookupdata, lookupdiv, lookuptitle, tablename, nameoftable, dataontable, nameofthelist, classoridoftheonwhichpopupderived, multipleallowedornot, nameidclassofbuttontodisable, setnameofthelookupbyppage, pagename) {
            //----------HTML-------------Example---------------
            //    <div class="dialog">
            //    <div title="LookUp Data">
            //        <div class="lookupdiv"></div>
            //    </div>
            //    <div id="pageNavPosition">

            //    </div>
            //</div>
            var data;
            nameidclassofbuttontodisable = ".popup-content-icon-edit,.popup-content-icon-remove";
            function pageon() {
                //--------------------------------initial---------------------
                window.pager = new Pager(setnameofthelookupbyppage, dataontable);
                pager.init();
                pager.showPageNav('pager', pagename);
                pager.showPage(1);
                //--------------------------------catch---------------------------
                function Pager(tableName, itemsPerPage) {
                    this.tableName = tableName;
                    this.itemsPerPage = itemsPerPage;
                    this.currentPage = 1;
                    this.pages = 0;
                    this.inited = false;

                    this.showRecords = function (from, to) {
                        var rows = document.getElementById(tableName).rows;
                        // i starts from 1 to skip table header row
                        for (var i = 1; i < rows.length; i++) {
                            if (i < from || i > to)
                                rows[i].style.display = 'none';
                            else
                                rows[i].style.display = '';
                        }
                    }

                    this.init = function () {
                        var rows = document.getElementById(tableName).rows;
                        var records = (rows.length - 1);
                        this.pages = Math.ceil(records / itemsPerPage);
                        this.inited = true;
                    }

                    this.showPageNav = function (pagerName, positionId) {
                        if (!this.inited) {
                            alert("not inited");
                            return;
                        }
                        var element = document.getElementById(positionId);
                        var pagerHtml = '<span onclick="' + pagerName + '.prev();" class="pg-normal"><span style="cursor:pointer">&#x21E6; Prev</span></span> | ';
                        for (var page = 1; page <= this.pages; page++)
                            pagerHtml += '<span id="pg' + page + '" class="pg-normal" onclick="' + pagerName + '.showPage(' + page + ');" style="cursor:pointer;">' + page + '</span> | ';
                        pagerHtml += '<span onclick="' + pagerName + '.next();" class="pg-normal"><span  style="cursor:pointer;">Next &#x21E8;</span></span>';

                        element.innerHTML = pagerHtml;
                    }
                    this.prev = function () {
                        if (this.currentPage > 1)
                            this.showPage(this.currentPage - 1);
                    }

                    this.next = function () {
                        if (this.currentPage < this.pages) {
                            this.showPage(this.currentPage + 1);
                        }
                    }
                    this.showPage = function (pageNumber) {
                        if (!this.inited) {
                            alert("not inited");
                            return;
                        }

                        var oldPageAnchor = document.getElementById('pg' + this.currentPage);
                        oldPageAnchor.className = 'pg-normal';

                        this.currentPage = pageNumber;
                        var newPageAnchor = document.getElementById('pg' + this.currentPage);
                        newPageAnchor.className = 'pg-selected';

                        var from = (pageNumber - 1) * itemsPerPage + 1;
                        var to = from + itemsPerPage - 1;
                        this.showRecords(from, to);
                    }
                }
            };
            var clickon = function () {
                var table = document.getElementById(setnameofthelookupbyppage);
                var tbody = table.getElementsByTagName("tbody")[0];
                tbody.onclick = function (e) {
                    e = e || window.event;
                    data = [];
                    var target = e.srcElement || e.target;
                    while (target && target.nodeName !== "TR") {
                        target = target.parentNode;
                    }
                    if (target && target.rowIndex > 0) {
                        var cells = target.getElementsByTagName("td");
                        for (var i = 0; i < cells.length; i++) {
                            data.push(cells[i].innerHTML);
                        }
                        var count = $('' + tablename + ' tbody')[0].childElementCount;
                        // var count = jQuery(tablename + ' tr').closest('tr').length;
                        console.log(count);
                        var inline = jQuery(tablename).find('td').eq(0).text();
                        if (multipleallowedornot == 'A') {
                            if (target.className != "selectedtr") {
                                target.className = "selectedtr";
                                console.log(classoridoftheonwhichpopupderived);
                                console.log(nameidclassofbuttontodisable);
                                jQuery(tablename).append('<tr tabindex="-1"><td><input type="text" name="' + nameofthelist + '" value="' + data[0] + '"/>' + data[0] + '</td><td>' + data[1] + '</td></tr>').insertAfter($(this).closest('tr')).TableOnRowsClick(nameoftable);
                                jQuery(classoridoftheonwhichpopupderived).find('#' + nameofthelist + '').parents('div').next('.icon-row').find(nameidclassofbuttontodisable).button('enable').removeClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                                jQuery(tablename + ' tr td').addClass("selectedtr");
                                setTimeout(function () {
                                    jQuery(tablename + '  tr td ').removeClass("selectedtr")
                                }, 500);
                            }
                            else {
                                target.className = "";
                                jQuery(tablename + ' tr td:contains(' + data[0] + ')').closest('tr').remove();
                                //console.log(count);
                                console.log(classoridoftheonwhichpopupderived);
                                console.log(nameidclassofbuttontodisable);
                                if (jQuery(tablename + ' tr').closest('tr').length == 1) {
                                    jQuery(classoridoftheonwhichpopupderived).find('#' + nameofthelist + '').parents('div').next('.icon-row').find(nameidclassofbuttontodisable).button('disable').addClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                                }
                            }
                        }
                        else if (multipleallowedornot == 'N') {
                            if (count >= 2) {
                                if (target.className == "selectedtr") {
                                    target.className = "";
                                    jQuery(tablename + ' tr td:contains(' + data[0] + ')').closest('tr').remove();
                                    jQuery(classoridoftheonwhichpopupderived).find('#' + nameofthelist + '').parents('div').next('.icon-row').find(nameidclassofbuttontodisable).button('disable').addClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                                }
                                else if (target.className != "selectedtr" && inline == data[0]) {
                                    target.className = "";
                                    jQuery(tablename + ' tr td:contains(' + data[0] + ')').closest('tr').remove();
                                    jQuery(classoridoftheonwhichpopupderived).find('#' + nameofthelist + '').parents('div').next('.icon-row').find(nameidclassofbuttontodisable).button('disable').addClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                                }
                                else {
                                    var newDivma = $(document.createElement('div'));
                                    var htmltag = '<p><span class="ui-icon ui-icon-alert" style="float:left;margin-right:10px"></span> Sorry Only One Record is Allowed to Write.';
                                    htmltag += '</p>';
                                    newDivma.html(htmltag);
                                    newDivma.dialog({
                                        autoOpen: false,
                                        title: "Information",
                                        height: 'auto', width: 'auto', modal: true,
                                        buttons: {
                                            Ok: function () {
                                                newDivma.dialog("close");
                                            }
                                        }
                                    });
                                    newDivma.dialog('open');
                                    $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                                }
                            }
                            else {
                                if (target.className != "selectedtr") {
                                    target.className = "selectedtr";
                                    //console.log("dsad");
                                    //jQuery('' + tablename + ' tr:last').after('<tr tabindex="-1"><td><input type="text" name="' + nameofthelist + '" value="' + firstdataparameter + '"/>' + firstdataparameter + '</td><td>' + seconddataparameter + '</td></tr>');
                                    jQuery('' + tablename + ' tr:last').after('<tr tabindex="-1"><td><input type="text" name="' + nameofthelist + '" value="' + data[0] + '"/>' + data[0] + '</td><td>' + data[1] + '</td></tr>').TableOnRowsClick(nameoftable);
                                    jQuery(classoridoftheonwhichpopupderived).find('#' + nameofthelist + '').parents('div').next('.icon-row').find(nameidclassofbuttontodisable).button('enable').removeClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                                    jQuery(tablename + ' tr td').addClass("selectedtr");
                                    setTimeout(function () {
                                        jQuery(tablename + '  tr td ').removeClass("selectedtr")
                                    }, 500);
                                }
                                else {
                                    target.className = "";
                                    jQuery(tablename + ' tr td:contains(' + data[0] + ')').closest('tr').remove();
                                    if (count == 1) {
                                        jQuery(classoridoftheonwhichpopupderived).find('#' + nameofthelist + '').parents('div').next('.icon-row').find(nameidclassofbuttontodisable).button('disable').addClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                                    }
                                }
                            }
                        }
                    }
                }
            };
            var searchon = function () {
                $('#lookup-search').on('keyup', function () {
                    var value = jQuery(this).val().toUpperCase().toLowerCase();
                    var $tablerows = $('#' + setnameofthelookupbyppage + ' tr');
                    if (value === '') {
                        $tablerows.show(500);
                        pageon();
                        $('#' + pagename + '').show();
                        return false
                    }
                    $tablerows.each(function (index) {
                        if (index !== 0) {

                            $tablerows = $(this);
                            var column2 = $tablerows.find("td").eq(1).text().toUpperCase().toLowerCase();
                            if ((column2.indexOf(value) > -1)) {
                                $tablerows.show(500);
                            }
                            else {
                                $tablerows.hide(500);
                            }
                        }
                        $('#' + pagename + '').hide();
                    });
                });
            };

            var lookupajaxdata;
            var dia = jQuery(this).dialog({
                autoOpen: false,
                width: 'auto',
                height: 'auto',
                modal: true,
                title: lookuptitle,
                closeOnEscape: false,
                beforeClose: function () {
                    jQuery(dia).dialog('destroy').hide();
                },
                open: function (event, ui) {
                    var ids = CountTableIds(tablename);
                    console.log(ids);
                    lookupajaxdata = $.ajax({
                        url: lookupurl,
                        type: 'POST',
                        cache: false,
                        contentType: 'application/json',
                        datatype: 'json',
                        data: JSON.stringify({ SkipIds: ids }),

                    });
                    lookupajaxdata.done(function (successdata) {
                        var htmltag = '<div style=" margin: 0px auto; width: 181px; padding: 1px 0px 5px;"><label >Search : </label><input type="text" id="lookup-search" /></div><table class="lookuptable" id="' + setnameofthelookupbyppage + '"><tr><th>Sr.No</th><th>Description</th></tr>'
                        jQuery.each(successdata, function (i, k) {
                            htmltag += '<tr tabindex="-1">' +
                                '<td>' + k.srno + '</td>' +
                                '<td>' + k.lookupvalue + '</td>' +
                                '</tr>'
                        });
                        htmltag += '</table>';
                        jQuery('.' + lookupdiv + '').html(htmltag);
                        clickon();
                        pageon();
                        searchon();
                    });
                },
                buttons: {
                    Ok: function () {
                        jQuery(dia).dialog('destroy').hide();
                    }
                }
            });
            jQuery(dia).dialog('open');
        };

        function LookupTableSelectedRowOnClick(tabledvalues) {
            var table = document.getElementById(tabledvalues);
            var tbody = table.getElementsByTagName("tbody")[0];
            var all_child = $(tbody)[0].children;
            tbody.onclick = function (e) {
                e = e || window.event;
                var target = e.srcElement || e.target;
                console.log(target);
                while (target && target.nodeName !== "TR") {
                    target = target.parentNode;
                }
                if (target && target.rowIndex > 0) {
                    if (target.className != "selectedtr") {
                        $.each(all_child, function (e) {
                            $(this).removeClass('selectedtr');
                        });
                        target.className = "selectedtr";
                    }
                    else {
                        target.className = "";
                    }
                }
            };

        };
        $.fn.TableOnRowsClick = function (tabledvalues) {
            //console.log(tabledvalues);
            var table = document.getElementById(tabledvalues);
            var tbody = table.getElementsByTagName("tbody")[0];
            var all_child = $(tbody)[0].children;
            tbody.onclick = function (e) {
                e = e || window.event;
                var target = e.srcElement || e.target;
                //console.log(target);
                while (target && target.nodeName !== "TR") {
                    target = target.parentNode;
                }
                if (target && target.rowIndex > 0) {
                    if (target.className != "selectedtr") {
                        $.each(all_child, function (e) {
                            $(this).removeClass('selectedtr');
                        });
                        target.className = "selectedtr";
                    }
                    else {
                        target.className = "";
                    }
                }
            };
        };
        $.fn.P2BGetTableDataonSelectedRow = function (tableidorclass, indexvalueoftable) {
            var data = [];
            $.each($("" + tableidorclass + " tr.selectedtr"), function () { //tr which has selected class which is delcared in standard css as standardtr.
                if (jQuery(this).find('td').eq(indexvalueoftable).text() != "") {
                    data.push(jQuery(this).find('td').eq(indexvalueoftable).text());
                }
            });
            return data;
        };

        $.fn.P2BLookUpEncapsulate = function (tablename, nameofthelist, firstdataparameter, seconddataparameter, nameoftable, nameofthebtndisable, multiple_allowed_or_not) {
            if (firstdataparameter == undefined || firstdataparameter == '' || firstdataparameter == null || seconddataparameter == undefined || seconddataparameter == '' || seconddataparameter == null) {
                return null;
            }
            else {
                //Value For Para multiple_allowed_or_not
                //A = Allowed;
                //N= Not Allowed;
                if (multiple_allowed_or_not == 'N') {
                    var count = $('' + tablename + ' tbody')[0].childNodes.length;
                    if (count == 3) {
                        $('<div></div>').P2BMessageModalDialog('ui-icon-alert', 'Only One ReCord Allowed...!');
                    } else {
                        if (firstdataparameter.length && seconddataparameter.length > 1) {
                            //alert("P2BLookUpEncapsulate-Set to N(Multiple Not Allowed) \n Assign data Not in correct formate\n or else set to A")
                        }
                        $(document).find(nameofthebtndisable).button('enable');
                        jQuery('' + tablename + ' tr:last').after('<tr tabindex="-1"><td><input type="text" name="' + nameofthelist + '" value="' + firstdataparameter + '"/>' + firstdataparameter + '</td><td>' + seconddataparameter + '</td></tr>');
                    }
                } else if (multiple_allowed_or_not == 'A') {
                    console.log(typeof firstdataparameter);
                    console.log(typeof seconddataparameter);
                    if (typeof seconddataparameter == 'string') {
                        console.log(nameofthelist);
                        $(document).find(nameofthebtndisable).button('enable');
                        jQuery('' + tablename + ' tr:last').after('<tr tabindex="-1"><td><input type="text" name="' + nameofthelist + '" value="' + firstdataparameter + '"/>' + firstdataparameter + '</td><td>' + seconddataparameter + '</td></tr>');
                        LookupTableSelectedRowOnClick(nameofthelist);
                    }
                    if (typeof firstdataparameter == 'object' && typeof seconddataparameter == 'object') {
                        $(document).find(nameofthebtndisable).button('enable');
                        for (var i = 0; i < firstdataparameter.length && seconddataparameter.length; i++) {
                            jQuery('' + tablename + ' tr:last').after('<tr tabindex="-1"><td><input type="text" name="' + nameofthelist + '" value="' + firstdataparameter[i] + '"/>' + firstdataparameter[i] + '</td><td>' + seconddataparameter[i] + '</td></tr>');
                            if (i == 0) {
                                LookupTableSelectedRowOnClick(nameofthelist);
                            }
                            //  }
                            //} else {
                            //  alert("check return parameter is not in correct format or it has null value");
                        }
                        //  } else {
                        //    alert("Worngly Pass Paramenter For P2BLookUpEncapsulate");
                    }
                }
                var count = $('' + tablename + ' tbody')[0].childNodes.length;
                if (count == 3) {
                    LookupTableSelectedRowOnClick(nameofthelist);
                }
            }
        };

        $.fn.P2BSelectMenuOnChange = function (onchangeevent, url, filter_drop, data2) {
            var init = jQuery(this);
            jQuery(init).off(onchangeevent).on(onchangeevent, function () {
                jQuery(filter_drop).empty().append("<option value=0 selected=true>-Select-</option>").selectmenu('refresh');
                var value = jQuery(init).val();
                if (value != 0) {
                    $.post(url, { data: value, data2: data2 }, function (data) {
                        $.each(data, function (i, k) {
                            jQuery(filter_drop).append($('<option>', {
                                value: k.Value,
                                text: k.Text,
                                selected: k.Selected
                            }));
                            jQuery(filter_drop).selectmenu("refresh").selectmenu("menuWidget").css({ "height": "auto" });
                        });
                    });
                } else {
                    //console.log($(filter_drop).empty());
                    jQuery(filter_drop).empty()
                        .append("<option value=0 selected=true>-Select-</option>")
                        .css({ "height": "auto" });
                }
            });
        };
        $.fn.P2BMessageModalDialog = function (ui_icon, Message) {
            var newDivma = $(document.createElement('div'));
            var htmltag = '<p><span class="ui-icon ' + ui_icon + '" style="float:left;margin-right:10px"></span>' + Message + '';
            htmltag += '</p>';
            newDivma.html(htmltag);
            newDivma.dialog({
                autoOpen: false,
                title: "Information",
                height: 'auto', width: 'auto', modal: true,
                buttons: {
                    Ok: function () {
                        newDivma.dialog("close");
                    }
                }
            });
            newDivma.dialog('open');
            $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
        };
        $.fn.P2BTransactionTableAnimation = function () {
            var init = jQuery(this);
            var ExpandAnimation = function (init) {
                var transcation_div = $(init).parent('.transactiondiv');

                //console.log(transcation_div);
                //console.log(init);

                $(transcation_div).find('button').hide();
                $(transcation_div).find('div').hide();
                $(init).hide();

                var heading = $(init).find('tr th:eq(1)').text();
                //console.log(heading);

                $(transcation_div).append('<span class="filter_title">' + heading + ' Filter</span>');

                $(transcation_div).find('span i').on('click', function () {
                    $(transcation_div).find('div').slideToggle("fast");
                    $(init).toggle("show");
                    $(transcation_div).find('span i').toggleClass('fa-rotate-180');

                    if ($(transcation_div).find('span i').hasClass('fa-rotate-180')) {
                        $(transcation_div).height(290);
                        $(transcation_div).find('span.filter_title').hide();
                        $(transcation_div).find('button').show();
                    }
                    else {
                        $(transcation_div).height(43);
                        $(transcation_div).find('button').hide();
                        $(transcation_div).find('span.filter_title').show();
                    }
                });
            };
            ExpandAnimation(init);
        };
        var RemoveElementTranscationtable = function (tablename) {
            $(tablename).on("click", "tr td span.transcation_delete", function (e) {
                $(this).parents('tr').remove();
            });
        }
        $.fn.checked = function (value) {

            if (value === true || value === false) {
                $(this).each(function () { this.checked = value; }).closest('tr').addClass('selectedtr');

            } else if (value === undefined || value === 'toggle') {
                $(this).each(function () { this.checked = !this.checked; });
            }
        };
        $.fn.P2BTransactionTableSelected = function (data) {
            var init = $(this);
            $.each(data, function (i, k) {
                var x = $(init).find("tr td input[type=checkbox][value=" + k + "]");
                //console.log(x);
                if (x != undefined) {
                    $(x).checked(true);
                }
            });
        };
        $.fn.AddDataToTranscation = function (tablename, lookupurl, NoOfRecordToShow, setnameofthelookupbyppage, lookupdiv, lookuppagename, pagename) {
            function pageon() {
                //--------------------------------initial---------------------
                var dataontable = NoOfRecordToShow || 5;
                window.pager = new Pager(setnameofthelookupbyppage, dataontable);
                pager.init();
                pager.showPageNav('pager', pagename);
                pager.showPage(1);
                //--------------------------------catch---------------------------
                function Pager(tableName, itemsPerPage) {
                    this.tableName = tableName;
                    this.itemsPerPage = itemsPerPage;
                    this.currentPage = 1;
                    this.pages = 0;
                    this.inited = false;

                    this.showRecords = function (from, to) {
                        var rows = document.getElementById(tableName).rows;
                        // i starts from 1 to skip table header row
                        for (var i = 1; i < rows.length; i++) {
                            if (i < from || i > to)
                                rows[i].style.display = 'none';
                            else
                                rows[i].style.display = '';
                        }
                    }

                    this.init = function () {
                        var rows = document.getElementById(tableName).rows;
                        var records = (rows.length - 1);
                        this.pages = Math.ceil(records / itemsPerPage);
                        this.inited = true;
                    }

                    this.showPageNav = function (pagerName, positionId) {
                        if (!this.inited) {
                            alert("not inited");
                            return;
                        }
                        var element = document.getElementById(positionId);
                        var pagerHtml = '<span onclick="' + pagerName + '.prev();" class="pg-normal"><span style="cursor:pointer">&#x21E6; Prev</span></span> | ';
                        for (var page = 1; page <= this.pages; page++)
                            pagerHtml += '<span id="pg' + page + '" class="pg-normal" onclick="' + pagerName + '.showPage(' + page + ');" style="cursor:pointer;">' + page + '</span> | ';
                        pagerHtml += '<span onclick="' + pagerName + '.next();" class="pg-normal"><span  style="cursor:pointer;">Next &#x21E8;</span></span>';

                        element.innerHTML = pagerHtml;
                    }
                    this.prev = function () {
                        if (this.currentPage > 1)
                            this.showPage(this.currentPage - 1);
                    }

                    this.next = function () {
                        if (this.currentPage < this.pages) {
                            this.showPage(this.currentPage + 1);
                        }
                    }
                    this.showPage = function (pageNumber) {
                        if (!this.inited) {
                            alert("not inited");
                            return;
                        }

                        var oldPageAnchor = document.getElementById('pg' + this.currentPage);
                        oldPageAnchor.className = 'pg-normal';

                        this.currentPage = pageNumber;
                        var newPageAnchor = document.getElementById('pg' + this.currentPage);
                        newPageAnchor.className = 'pg-selected';

                        var from = (pageNumber - 1) * itemsPerPage + 1;
                        var to = from + itemsPerPage - 1;
                        this.showRecords(from, to);
                    }
                }
            };
            var clickon = function () {
                var table = document.getElementById(lookuppagename);
                var tbody = table.getElementsByTagName("tbody")[0];
                tbody.onclick = function (e) {
                    e = e || window.event;
                    var data = [];
                    var target = e.srcElement || e.target;
                    while (target && target.nodeName !== "TR") {
                        target = target.parentNode;
                    }
                    if (target && target.rowIndex > 0) {
                        var cells = target.getElementsByTagName("td");
                        for (var i = 0; i < cells.length; i++) {
                            data.push(cells[i].innerHTML);
                        }
                        if (target.className != "selectedtr") {
                            target.className = "selectedtr";
                            jQuery('#' + tablename + '').append('<tr tabindex="1"><td><input type="checkbox" class="case" name=' + tablename + ' value=' + data[0] + ' /></td><td style="display:none;">' + data[0] + '</td><td>' + data[1] + '<span title="Delete" class="transcation_delete" style="float:right">&#9932;</span></td></tr>').insertAfter($('#' + tablename + '').closest('tr'));
                        }
                        else {
                            target.className = "";
                            jQuery('#' + tablename + ' tr td:contains(' + data[0] + ')').closest('tr').remove();
                        }
                    }
                }
                RemoveElementTranscationtable('#' + tablename + '');
            };
            var searchon = function () {
                $('#lookup-search').on('keyup', function () {
                    var value = jQuery(this).val().toUpperCase().toLowerCase();
                    var $tablerows = $('#' + setnameofthelookupbyppage + ' tr');
                    if (value === '') {
                        $tablerows.show(500);
                        pageon();
                        $('#' + pagename + '').show();
                        return false
                    }
                    $tablerows.each(function (index) {
                        if (index !== 0) {

                            $tablerows = $(this);
                            var column2 = $tablerows.find("td").eq(1).text().toUpperCase().toLowerCase();
                            if ((column2.indexOf(value) > -1)) {
                                $tablerows.show(500);
                            }
                            else {
                                $tablerows.hide(500);
                            }
                        }
                        $('#' + pagename + '').hide();
                    });
                });
            };
            var lookupajaxdata;
            var init = jQuery(this).dialog({
                autoOpen: false,
                width: 'auto',
                height: 'auto',
                modal: true,
                title: "Lookup Model",
                closeOnEscape: false,
                open: function (event, ui) {
                    $(".ui-dialog-titlebar-close", ui.dialog | ui).on('click', function () {
                        jQuery(init).dialog('destroy').hide();
                    });
                    var ids = CountTranscationTableIds(tablename);
                    console.log(ids);
                    lookupajaxdata = $.ajax({
                        url: lookupurl,
                        type: 'POST',
                        cache: false,
                        contentType: 'application/json',
                        datatype: 'json',
                        data: JSON.stringify({ SkipIds: ids })
                    });
                    lookupajaxdata.done(function (successdata) {
                        var htmltag = '<div style=" margin: 0px auto; width: 181px; padding: 1px 0px 5px;"><label >Search : </label><input type="text" id="lookup-search" /></div><table class="lookuptable" id="' + setnameofthelookupbyppage + '"><tr><th>Sr.No</th><th>Description</th></tr>'
                        jQuery.each(successdata, function (i, k) {
                            htmltag += '<tr tabindex="-1">' +
                                '<td>' + k.srno + '</td>' +
                                '<td>' + k.lookupvalue + '</td>' +
                                '</tr>'
                        });
                        htmltag += '</table>';
                        jQuery('.' + lookupdiv + '').html(htmltag);
                        clickon();
                        pageon();
                        searchon();
                    });
                },
                buttons: {
                    Ok: function () {
                        jQuery(this).dialog('destroy').hide();
                    }
                }
            });
            jQuery(this).dialog('open');
        };
        $.fn.P2BTransactionTableDynamic = function (IDofinputsearch, IDofCheckbox, urldatatoload, forwardata, single) {
            var init = $(this);
            var datatoserverside = [];
            if (forwardata == '' || forwardata == null) {
                datatoserverside = ["9999999999"];
            }
            else {
                datatoserverside = forwardata
            }
            var searchon = function () {
                jQuery(IDofinputsearch).on('keyup', function (e) {
                    var value = jQuery(this).val().toUpperCase().toLowerCase();
                    var $rows = $('#' + init.attr('id') + ' tr');
                    if (value == '') {
                        $rows.show(500);
                        return false;
                    }
                    $rows.each(function (index) {
                        if (index !== 0) {
                            var $row = $(this);
                            var column2 = $row.find("td").eq(2).text().toUpperCase().toLowerCase();
                            if ((column2.indexOf
                                (value) > -1)) {
                                $row.show(500);
                            }
                            else {
                                $row.hide(500);
                            }
                        }
                    });
                });
            };
            function checkboselection() {

                $(IDofCheckbox).on('click', function (e) {
                    single = single || false;
                    if (single == false) {

                        var b = init.attr('id');
                        var count = $("#" + b + " tr").filter(function () {
                            return $(this).css('display') == 'none';
                        }).length;
                        if (this.checked) {
                            $('#' + b + ' .case').each(function () {
                                if (count > 0) {
                                    var data = $("#" + b + " tr").filter(function () {
                                        return $(this).css('display') == 'none';
                                    }).find('.case').removeAttr('checked').checked = false;
                                    var data_1 = $("#" + b + " tr:not(:first-child)").filter(function () {
                                        return $(this).css('display') !== 'none';
                                    }).addClass('selectedtr').find('.case').attr('checked', 'checked').prop('checked', true).checked = true;
                                    if (this.checked) {
                                        var abcdert = jQuery(this).val();
                                        console.log(value);

                                    }
                                }
                                else {
                                    $('#' + b + ' .case').attr('checked', 'checked');
                                    this.checked = true;
                                    var value_checked = jQuery('#' + b + ' .case:checked').parent('td').parent('tr');
                                    value_checked.addClass('selectedtr');
                                    if (this.checked) {
                                        var abcd = jQuery(this).val();
                                    }
                                    else {
                                        alert('No Data Found');
                                    }
                                }
                            });
                        } else {
                            $('#' + b + ' .case').each(function (e) {
                                var value_unchecked = jQuery(this).parent('td').parent('tr');
                                value_unchecked.removeClass('selectedtr');
                                $('#' + b + ' .case').removeAttr('checked');
                                this.checked = false;
                            });
                        }
                    } else {
                        return false;
                    }
                });
                $(document).on('click', '#' + init.attr('id') + ' .case', function (e) {
                    single = single || false;
                    if (single == false) {
                        if (this.checked) {
                            var value_checked = jQuery('.case:checked').parent('td').parent('tr');
                            value_checked.addClass('selectedtr');
                        }
                        else {
                            console.log('a');
                            var value_unchecked = jQuery(this).parent('td').parent('tr');
                            value_unchecked.removeClass('selectedtr');
                        }
                    } else {
                        if (this.checked) {
                            var value_checked = jQuery('.case:checked').parent('td').parent('tr');
                            value_checked.addClass('selectedtr');
                            $('.case:checked').not(this).prop('checked', false).parent('td').parents('tr:eq(0)').removeClass('selectedtr');
                        } else {
                            var value_unchecked = jQuery(this).parent('td').parent('tr');
                            value_unchecked.removeClass('selectedtr');
                        }
                    }
                    if ($('.case:checked').length == $('.case').length) {
                        $(IDofCheckbox).prop('checked', true);
                    } else {
                        $(IDofCheckbox).prop('checked', false);
                    }
                });

            };
            if (urldatatoload != "") {
                var dataload = jQuery.ajax({
                    url: urldatatoload,
                    type: 'POST',
                    cache: false,
                    data: { data: datatoserverside.toString() }
                });
                dataload.done(function (data) {
                    if (data.data != null) {
                        $('#' + data.tablename + '').parent('div.transactiondiv').parents('div').show();
                        $('#' + data.tablename + '').find('td').remove();
                        console.log($('#' + data.tablename + ''));
                        $.each(data.data, function (i, k) {
                            jQuery('#' + data.tablename + '').append('<tr tabindex="1"><td><input type="checkbox" class="case" name=' + data.tablename + ' value=' + k.code + ' /></td><td style="display:none;">' + k.code + '</td><td>' + k.value + '</td></tr>').insertAfter(jQuery('#' + data.tablename + '').closest('tr'));
                        });
                    } else {
                        if (data.responseText != null) {
                            alert(data.responseText);
                        }
                    }
                });
            }
            searchon();
            checkboselection();
        };
        var TranscationTableCheckboxCount = function (table) {
            return $(table).find('input:checkbox:gt(0):checked').length;
        };
        $.ApplyFilter = function (url, inputname, formname, callback) {
            formname = formname || "#frmtrasctionform";
            $.ajax({
                method: "POST",
                url: url,
                data: $(formname).serialize(),
                success: function (data) {
                    console.log(typeof data);
                    if (typeof data != "string" && data != "") {
                        $(inputname).val(data);
                        console.log($(inputname).val());
                        if (typeof callback === "function") {
                            callback.call();
                        }
                    } else {
                        alert(data.toString());
                        if (typeof callback === "function") {
                            callback.call();
                        }
                    }
                }
            });
        };
        $.fn.DynamicSelectMenuAppend = function (Url, SelectType) {
            var init = jQuery(this);
            jQuery("#" + $(this).attr('id') + "").empty().append("<option value=0 selected=true>-Select-</option>").selectmenu('refresh');
            var value = jQuery(init).val();
            $.post(Url, { data: value, data2: SelectType }, function (data) {
                if (data.SelectlistType != null) {
                    $('#' + data.SelectlistType + '').parent().show();
                    $.each(data.selectlist, function (i, k) {
                        jQuery('#' + data.SelectlistType + '').append($('<option>', {
                            value: k.Value,
                            text: k.Text,
                            selected: k.Selected
                        }));
                        jQuery('#' + data.SelectlistType + '').selectmenu("refresh").selectmenu("menuWidget").addClass("overflow");
                    });
                }
            });
        };
        $.fn.DynamicSelectMenuOnChange = function (Url) {
            var init = jQuery(this);
            jQuery(init).on("selectmenuchange", function () {
                var value = jQuery("#" + $(this).attr('id') + "").val();
                var SelectType = $(this).attr('id').substr(0, $(this).attr('id').indexOf("_"));
                console.log(value);
                if (value != 0) {
                    $.post(Url, { data: value, data2: SelectType }, function (data) {
                        if (data.SelectlistType != null) {
                            $('#' + data.SelectlistType + '').parent().show();
                            jQuery("#" + data.SelectlistType + "").empty().append("<option value=0 selected=true>-Select-</option>").selectmenu('refresh');
                            $.each(data.selectlist, function (i, k) {
                                jQuery('#' + data.SelectlistType + '').append($('<option>', {
                                    value: k.Value,
                                    text: k.Text,
                                    selected: k.Selected
                                }));
                                jQuery('#' + data.SelectlistType + '').selectmenu("refresh").selectmenu("menuWidget").addClass("overflow");
                            });
                        }
                    });
                } else {
                    //console.log($(filter_drop).empty());
                    jQuery('#' + data.SelectlistType + '').empty()
                        .append("<option value=0 selected=true>-Select-</option>")
                        .selectmenu("refresh")
                    ;
                }
            });
        };
        $.fn.InlineGridEdittedData = {
            arr: [],
            SetData: function (data) {
                if (this.arr.length > 0) {
                    var temp = this.arr;
                    for (var i = 0; i < temp.length; i++) {
                        if (data.Id == temp[i].Id) {
                            temp.splice(i, 1);
                        }
                    }
                }
                this.arr.push(data);
                console.log(this.arr);
            },
            GetData: function (data) {
                if (this.arr.length > 0) {
                    return this.arr;
                } else {
                    return null;
                }
            },
            Empty: function () {
                this.arr.length = 0;
            }
        };
        $.fn.InlineEditGrid = function (fn) {
            var obj = $.extend({
                ColNames: [],
                ColModel: [],
                SortName: null,
                Caption: null,
                url: null,
                tablename: null,
                eddelurl: null,
                width: null,
                height: null,
                forwarddata: null,
                inlinePager: null,
                EditableCol: null,
                CheckCol: null,
            }, fn);
            var editrow, table, celValue, lastsel, colmodelname = [], colmodeldata = [];
            //inline_jqgridemp_pager
            obj.inlinePager = obj.tablename + "_pager" || '#inline_pager';
            table = obj.tablename;
            for (var i = 0; i < obj.ColNames; i++) {
                colmodelname.push(obj.ColNames[i]);
            }
            for (var j = 0; j < obj.ColModel.length; j++) {
                colmodeldata.push({ name: obj.ColModel[j], index: obj.ColModel[j], width: 150, align: "center", editable: true, editoptions: { size: 10 } });
            }
            this.jqGrid({
                url: obj.url + '?extraeditdata=' + obj.forwarddata + '',
                gridview: true,
                rownumbers: true,
                cellEdit: true,
                cellsubmit: 'clientArray',
                datatype: "json",
                colNames: colmodelname,
                colModel: colmodeldata,
                rowNum: 10,
                rowList: [10, 20, 30],
                pager: obj.inlinePager,
                sortname: obj.SortName,
                viewrecords: true,
                sortorder: "asc",
                caption: obj.Caption,
                afterSaveCell: function (data) {
                    var a = $(this).getRowData(data);
                    $(this).InlineGridEdittedData.SetData(a);
                },
                search: {
                    odata: ['equal']
                },
                width: obj.width,
                height: obj.height,
                editurl: "clientArray",
                //beforeEditCell: function (rowid, cellname, value, iRow, iCol) {
                //	//$("#inline_jqgrid").editCell(iRow, iCol,true);
                //	//var a = $("#jqGrid").getRowData(iRow);

                //	//if (a[obj.CheckCol] == "1") {

                //	//}
                //	// here identify row based on rowid
                //	// if the row should not be editable than simply make the cells noneditable using
                //	//editCell(iRow, iCol, false);
                //	//jQuery("#inline_jqgrid").jqGrid("restoreCell", iRow, iCol);

                //},
                beforeSelectRow: function (rowid, e) {
                    var $td = $(e.target).closest("td"), iCol = $.jgrid.getCellIndex($td[0]);
                    var a = $(table).getRowData(rowid);
                    console.log(iCol);
                    console.log(obj.ColModel.indexOf(obj.EditableCol[0]));
                    console.log(a[obj.CheckCol]);
                    if (a[obj.CheckCol] == "true" && obj.ColModel.indexOf(obj.EditableCol[0]) + 1 == iCol) {
                        $td.removeClass('not-editable-cell');
                        return false;
                    } else {
                        $td.addClass('not-editable-cell');
                        return false;
                    }
                },
                gridComplete: function () {
                    //var id = jQuery(this).jqGrid().getDataIDs(), a = [];
                    //$.each(id, function (i, k) {
                    //	var a = $("#inline_jqgrid").getRowData(k);
                    //	if (a[obj.CheckCol] == "1") {
                    //		//var b = $("#inline_jqgrid").jqGrid('getColProp', obj.EditableCol[0]);

                    //	}
                    //});
                    //$.each(a, function (i, k) {
                    //    if (k[obj.CheckCol] == "true") {
                    //        console.log(k);
                    //        //for (var j = 0; j < obj.EditableCol.length; j++) {
                    //        //    var b = $("#inline_jqgrid").jqGrid('getColProp', obj.EditableCol[i]);
                    //        //    console.log(b);
                    //        //}
                    //    }
                    //});
                    //console.log(a[i]);
                    //var a = $(this).jqGrid('getColProp', obj.EditableCol[i]);
                    //if (obj.CheckCol != null) {
                    //    //jQuery(tablename).jqGrid('getRowData', id, value);
                    //    //var x=$(this).jqGrid()
                    //    console.log($(this).getGridParam("records"));
                    //}
                    //a.editable = true;
                    //}
                    //console.log(a[i]);
                    //var a = $(this).jqGrid('getColProp', obj.EditableCol[i]);
                    //if (obj.CheckCol != null) {
                    //    //jQuery(tablename).jqGrid('getRowData', id, value);
                    //    //var x=$(this).jqGrid()
                    //    console.log($(this).getGridParam("records"));
                    //}
                    //a.editable = true;
                    //}
                }
            });
            this.jqGrid('navGrid', obj.inlinePager, { edit: false, add: false, del: false });
            this.trigger('reloadGrid');
        };
        $.GetGridSelctedvalue = function (Gridname) {
            //console.log(Gridname);
            //alert("sdasd");
            var a = jQuery(Gridname).jqGrid('getGridParam', 'selarrrow');
            if (a.length != 0) {
                var selected_ids = [];
                for (var i = 0; i < a.length; i++) {
                    selected_ids.push(jQuery(Gridname).jqGrid('getCell', a[i], "Id"));
                }
                return selected_ids;
            } else {
                //alert("Select Row..!");
                return false;
            }
        };
        $.fn.P2BGridDialog = function (fn) {
            var obj = $.extend({
                height: 300,
                width: 500,
                title: "Dialog",
                editurl: null,
                editdata: null,
                returndata: null,
                gridname: null,
                gridfunction: null,
                form: null,
                forwarddata: null,
                gridreloadname: null,
                filter: null,
                state: null,
                returnToGrid: null,
                refreshgrid: null,
                submiturl: null
            }, fn);
            var init = $(this);
            var griddialog = $(this).dialog({
                autoOpen: false,
                height: obj.height,
                width: obj.width,
                modal: true,
                closeOnEscape: false,
                title: obj.title,
                beforeClose: function () {
                    obj.gridname != null ? $(obj.gridname).trigger("reloadGrid") : true;
                    obj.returnToGrid != null ? $(obj.returnToGrid).trigger("reloadGrid") : true;
                    $('.ui-dialog-buttonpane button:contains("Submit")').button("enable");
                },
                open: function (event, ui) {
                    if (obj.submiturl == null) {
                        $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('disable');
                    }
                    if (obj.editurl != null && obj.returndata != null && $.type(obj.returndata) === "function") {
                        $.ajax({
                            url: obj.editurl,
                            method: "POST",
                            data: { id: obj.editdata },
                            success: function (data) {
                                if (obj.returndata != null) {
                                    obj.returndata(data);
                                }
                            }
                        });
                    }
                    if (obj.returnToGrid != null) {
                        $(obj.returnToGrid).setGridParam({ url: obj.editurl, postData: { id: obj.editdata, filter: obj.filter }, page: 1 }).trigger("reloadGrid");
                    }
                },
                buttons: {
                    Submit: function () {
                        if (obj.submiturl == null) {
                            return false;
                        }
                        if (obj.gridname != null && obj.gridfunction != null) {
                            var a = $.GetGridSelctedvalue(obj.gridname);
                            if (a == true) {
                                obj.forwarddata = a;
                            } else if (JqGridCheck.select_all == true) {
                                console.log(JqGridCheck.select_all);
                                obj.forwarddata = JqGridCheck.Get();
                            }
                        }
                        if (obj.returnToGrid != null) {
                            var a = $(obj.returnToGrid).InlineGridEdittedData.GetData(obj.returnToGrid);
                            if (a != null) {
                                obj.forwarddata = JSON.stringify(a);
                            }
                        }
                        var ajaxdata = $.ajax({
                            url: obj.submiturl,
                            method: "POST",
                            beforeSend: function () {
                                $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('disable').addClass('submitbtndisable');
                                ajaxloaderv2('body');
                            },
                            data: $(obj.form).serialize() + "&forwarddata=" + obj.forwarddata + "&selected=" + obj.editdata
                        });
                        ajaxdata.success(function (msg) {
                            $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('enable').removeClass('submitbtndisable');
                            $('.ajax_loder').remove();
                            if (msg.success == true) {
                                var newDiv = $(document.createElement('div'));
                                var htmltag = '<p><span class="ui-icon ui-icon-check" style="float:left;margin-right:10px"></span> ' + msg.responseText + '';
                                htmltag += '</p>';
                                newDiv.html(htmltag);
                                newDiv.dialog({
                                    autoOpen: false,
                                    title: "Information",
                                    height: 150, width: 250, modal: true,
                                    buttons: {
                                        Ok: function (e) {
                                            if (obj.gridreloadname != null) {
                                                jQuery(obj.gridreloadname).trigger('reloadGrid');
                                            }
                                            if (obj.returnToGrid != null) {
                                                $(obj.returnToGrid).InlineGridEdittedData.Empty();
                                            }
                                            if (obj.refreshgrid != null) {
                                                var table = $(obj.refreshgrid).DataTable();
                                                table.ajax.reload();
                                            }
                                            newDiv.dialog("close");
                                            newDiv.remove();
                                            jQuery(griddialog).dialog("close");
                                        }
                                    }
                                });
                                newDiv.dialog('open');
                                $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                            } else {
                                var newDiv = $(document.createElement('div'));
                                var htmltag = '<p><span class="ui-icon ui-icon-alert" style="float:left;margin-right:10px"></span> ' + msg.responseText + '';
                                htmltag += '</p>';
                                newDiv.html(htmltag);
                                newDiv.dialog({
                                    autoOpen: false,
                                    title: "Information",
                                    height: 150, width: 250, modal: true,
                                    buttons: {
                                        Ok: function (e) {
                                            //if (obj.gridreloadname != null) {
                                            //    jQuery(obj.gridreloadname).trigger('reloadGrid');
                                            //}
                                            newDiv.dialog("close");
                                            newDiv.remove();
                                            //  jQuery(griddialog).dialog("close");
                                        }
                                    }
                                });
                                newDiv.dialog('open');
                                $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                            }

                        });
                        ajaxdata.error(function (xhr, status, error) {
                            $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('enable').removeClass('submitbtndisable');
                            $('.ajax_loder').remove();
                            var newDiv = $(document.createElement('div'));
                            var htmltag = '<p><span class="ui-icon ui-icon-alert" style="float:left;margin-right:10px"></span> ' + xhr.status + '"-"' + xhr.statusText + '';
                            htmltag += '</p>';
                            newDiv.html(htmltag);
                            newDiv.dialog({
                                autoOpen: false,
                                title: "Information",
                                height: 130,
                                width: 250,
                                modal: true,
                                buttons: {
                                    Ok: function () {
                                        if (obj.gridreloadname != null) {
                                            jQuery(obj.gridreloadname).trigger('reloadGrid');
                                        }
                                        if (obj.returnToGrid != null) {
                                            $(obj.returnToGrid).InlineGridEdittedData.Empty();
                                        }
                                        if (obj.refreshgrid != null) {
                                            var table = $(obj.refreshgrid).DataTable();
                                            table.ajax.reload();
                                        }
                                        newDiv.remove();
                                        newDiv.dialog("close");
                                        jQuery(griddialog).dialog("close");
                                    }
                                }
                            });
                            newDiv.dialog('open');
                            $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                        });
                    },
                    Cancel: function () {
                        jQuery(griddialog).dialog("close");
                    }
                }
            });
            jQuery(this).dialog(obj.state);
            $('.ui-dialog-buttonpane').find('button:contains("Submit")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-disk"></span>');
            $('.ui-dialog-buttonpane').find('button:contains("Cancel")').removeClass('ui-buttontext-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-closethick"></span>');
        };
        $.AdvanceFilterFunction = function (fn) {
            var obj = $.extend({
                appenddiv: null,
                url: null,
                bydefaultloademp: null,
                loadempurl: null,
                applyfiltercheckbox: null,
                minimizeid: null,
            }, fn);

            $.ajax({
                method: "GET",
                url: obj.url,
                success: function (htmldata) {
                    $(obj.appenddiv).html(htmldata);
                    $(obj.appenddiv).slideUp();
                }
            });

            //$(obj.applyfiltercheckbox).on('click', function (data) {
            //    var a = obj.applyfiltercheckbox.split("-");
            //    if (this.checked) {
            //        $(obj.applyfiltercheckbox).ApplyFilter('#employee-table', "./Transcation/Get_" + a[1] + "id", "#" + a[1] + "_id", "#" + a[1] + "");
            //    }

            //});
            $('' + obj.minimizeid + ' a').on('click', function (e) {
                $(this).find('i').toggleClass('fa-rotate-90');
                $(this).find('a').attr('data-p2bheadertooltip', $(this).find('a').attr('data-p2bheadertooltip') == 'Collapse' ? 'Expand' : 'Collapse');
                $(this).parents('legend').nextAll('div').slideToggle("slow");
                e.preventDefault();
            });
            //if (obj.loadempurl != null) {
            //    $('#employee-table').P2BTransactionTableDynamic('#employee-search', '#case-employee', obj.loadempurl, []);
            //}

        };
        $.RadioBtnGroup = function (Btn1, Btn2) {
            $("input:radio[name=" + Btn1 + "]").on('change', function (e) {
                $('input:radio[name=' + Btn2 + ']').val([false]).button('refresh');
            });
            $("input:radio[name=" + Btn2 + "]").on('change', function (e) {
                $('input:radio[name=' + Btn1 + ']').val([false]).button('refresh');
            });
        };
        $.OnCheckMakeTextboxDisable = function (RadioBtn, TextBox) {
            $("[name=" + RadioBtn + "]").on('click', function () {
                if ($("[name=" + RadioBtn + "]:checked").val() == "true") {
                    $(TextBox).removeAttr("disabled");
                } else {
                    $(TextBox).attr("disabled", true);
                }
            });
        };
        $.fn.MonthAndYear = function (MonthDaysTextBox) {
            $(this).attr("readonly", true);
            $(this).datepicker({
                changeMonth: true,
                changeYear: true,
                stepMonths: true,
                dateFormat: 'mm/yy',
                onSelect: function (dateText, inst) {
                    var month = 1 + parseInt($(".ui-datepicker-month :selected").val());
                    var year = $("#ui-datepicker-div .ui-datepicker-year :selected").val();
                    var days = new Date(year, month, 1, -1).getDate();
                    $(MonthDaysTextBox).val(days);
                }
            });
        };
        $.fn.ProcessConfirmation = function (fn) {
            var obj = $.extend({
                confirmurl: null,
                submiturl: null,
                month: null,
                gridid: null,
                cancelurl: null,
                status: null,
                msg: null,
            }, fn);
            var init = $(this);

            var typeofbtn = $(this).data("typeofbtn"), selectedid;
            if (obj.confirmurl != null) {
                $.ajax({
                    url: obj.confirmurl,
                    async: true,
                    data: { typeofbtn: typeofbtn, month: $(obj.month).val() },
                    beforeSend: function () {
                        $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('disable').addClass('submitbtndisable');
                        ajaxloaderv2('body');
                    },
                    success: function (data) {
                        $('.ajax_loder').remove();
                        if (data != null) {
                            if (data.status == true) {
                                var newdia = $("<div id='newDia'>" + obj.msg + "</div>");
                                newdia.dialog({
                                    autoOpen: false,
                                    height: 200,
                                    width: 350,
                                    title: "Confirm Box",
                                    model: true,
                                    beforeclose: function () {
                                        diahtml.remove();
                                    },
                                    buttons: {
                                        Confirm: function () {
                                            if (obj.gridid != null) {
                                                if (obj.gridid != null) {
                                                    selectedid = $.GetGridSelctedvalue(obj.gridid);
                                                }

                                                $(obj.gridid).setGridParam({ url: obj.submiturl + "?&month=" + $(obj.month).val() + "&typeofbtn=" + typeofbtn, postData: { filter: selectedid.toString() }, page: 1 }).trigger("reloadGrid");
                                            }
                                            newdia.dialog("close");
                                            if (typeof obj.status === 'function') {
                                                obj.status(true);
                                            }
                                        },
                                        Cancel: function () {
                                            if (typeof obj.status === 'function') {
                                                obj.status(false);
                                            }
                                            newdia.dialog("close");
                                            if (obj.gridid != null && obj.cancelurl != null) {
                                                $(obj.gridid).setGridParam({ url: obj.cancelurl, page: 1 }).trigger("reloadGrid");
                                            }
                                        }
                                    }
                                });
                                newdia.dialog('open');
                            } else {
                                if (obj.gridid != null) {
                                    if (obj.gridid != null) {
                                        selectedid = $.GetGridSelctedvalue(obj.gridid);
                                    }
                                    $(obj.gridid).setGridParam({ url: obj.submiturl + "?&month=" + $(obj.month).val() + "&typeofbtn=" + typeofbtn, postData: { filter: selectedid.toString() }, page: 1 }).trigger("reloadGrid");
                                } else {
                                    if (typeof obj.status === 'function') {
                                        obj.status(true);
                                    }
                                }
                            }
                        }
                    },
                    error: function (data) {
                        $('.ajax_loder').remove();
                        console.log(data);
                    }
                });
            }
        };
        $.fn.PostGridData = function (fn) {
            var obj = $.extend({
                filter: null,
                url: null,
            }, fn);
            $(this).setGridParam({ url: obj.url, postData: { filter: obj.filter } }).trigger('reloadGrid');
        };
        $.fn.MonthYearPicker = function () {
            return this.each(function () {
                $(this).datepicker({
                    changeMonth: true,
                    changeYear: true,
                    stepMonths: true,
                    dateFormat: 'mm/yy',
                });

            });
        };
        $.fn.FilterDialog = function (fn) {
            var obj = $.extend({
                width: null,
                height: null,
                title: null,
                htmlurl: null,
                appendat: null,
                returnat: null,
                renderat: null,
                form: "#advancefilter",
            }, fn);
            var init = $(this);
            function ajaxfun() {
                var ajx = $.ajax({
                    url: obj.htmlurl,
                    method: "GET",
                });
                ajx.success(function (html) {
                    if (obj.renderat != null) {
                        $(obj.renderat).append(html);
                        if ($('#advance-filter-symbol').hasClass('advance_filter_sysmbol_class_plus')) {
                            $('#advance-filter-symbol').removeClass('advance_filter_sysmbol_class_plus');
                            $('#advance-filter-symbol').addClass('advance_filter_sysmbol_class_minus');
                            $('#advance-filter-symbol').attr('title', "Collapse");
                        };
                        $('form#advancefilter').on('click', '#resetfilter', function (e) {
                            $(obj.renderat).empty();
                            ajaxfun();
                            e.preventDefault();
                        });
                        $('form#advancefilter').on('click', '#applyfilter', function (e) {
                            $.ApplyFilter("./Transcation/Get_Geoid", "#filter-geo-struct-id", "#geo", function () {
                                $.ApplyFilter("./Transcation/Get_Payid", "#filter-pay-struct-id", "#pay", function () {
                                    $.ApplyFilter("./Transcation/Get_Funid", "#filter-fun-struct-id", "#fun", function () {
                                        var data = {
                                            GeoStruct: $('form#advancefilter #filter-geo-struct-id').val() || null,
                                            PayStruct: $('form#advancefilter #filter-pay-struct-id').val() || null,
                                            FunStruct: $('form#advancefilter #filter-fun-struct-id').val() || null
                                        };
                                        alert('Filter Applied..!');
                                        obj.returnat(data);
                                    });
                                });
                            });
                            e.preventDefault();
                        });
                    }
                });
                ajx.error(function (data) {
                    console.log(data);
                });
            };
            if ($(obj.renderat)[0].childElementCount == 0) {
                ajaxfun();
            }
            if ($(obj.renderat).is(':visible')) {
                if ($('#advance-filter-symbol').hasClass('advance_filter_sysmbol_class_minus')) {
                    $('#advance-filter-symbol').removeClass('advance_filter_sysmbol_class_minus');
                    $('#advance-filter-symbol').addClass('advance_filter_sysmbol_class_plus');
                    $('#advance-filter-symbol').attr('title', "Expand");
                };
                $(obj.renderat).hide();
            } else {
                if ($('#advance-filter-symbol').hasClass('advance_filter_sysmbol_class_plus')) {
                    $('#advance-filter-symbol').removeClass('advance_filter_sysmbol_class_plus');
                    $('#advance-filter-symbol').addClass('advance_filter_sysmbol_class_minus');
                    $('#advance-filter-symbol').attr('title', "Collapse");
                };
                $(obj.renderat).show();

            };
        };
        $.fn.CustomeDialog = function (fn) {
            var obj = $.extend({
                htmlurl: null,
                onloaddataurl: null,
                onloaddataid: null,
                submiturl: null,
                editid: null,
                form: null,
                title: null,
                height: 473,
                width: 750,
                onloadreturnfunction: null,
                forwarddata: null,
                onsubmitreturnfunction: null
            }, fn);
            if (obj.form == null) {
                alert("form null");
                return false;
            }
            var init = $(this);
            var dia = $(init).dialog({
                autoOpen: false,
                height: obj.height,
                width: obj.width,
                modal: true,
                closeOnEscape: false,
                beforeClose: function () {
                    jQuery(init).remove();
                },
                title: obj.title,
                open: function (event, ui) {
                    function assigndata() {
                        var editajaxopenloaddata = $.ajax({
                            url: obj.onloaddataurl,
                            method: 'POST',
                            data: { data: obj.onloaddataid },
                        });
                        editajaxopenloaddata.done(function (value) {
                            obj.onloadreturnfunction(value);
                        });
                    };
                    jQuery.ajax({
                        url: obj.htmlurl,
                        method: "GET",
                        beforeSend: function () {
                            ajaxloader(init);
                            $(init).find("div .ajax_loder").show();
                        },

                        success: function (result, status, xhr) {
                            showajaxeroor(xhr, init);
                            $(init).find("div .ajax_loder").hide();
                            $(init).html(result);
                            assigndata();
                            OnEnterFocusNext();
                            AlterBtnType();
                        },
                        error: function (xhr, status, error) {
                            showajaxeroor(xhr, init);
                            $(init).find("div .ajax_loder").show();
                        },
                        complete: function () {
                            $(init).find("div .ajax_loder").hide();
                        }
                    });
                },
                buttons: {
                    Submit: function () {
                        var x = PerformValidations(obj.form);
                        if (x == true) {
                            var editajaxdata = $.ajax({
                                url: obj.submiturl,
                                method: "POST",
                                async: true,
                                data: $(obj.form).serialize() + '&data=' + obj.onloaddataid + '',
                                beforeSend: function () {
                                    // alert('hiihihihihi');
                                    $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('disable').addClass('submitbtndisable');
                                    ajaxloaderv2('body');

                                },
                            });
                            editajaxdata.success(function (msg) {
                                $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('enable').removeClass('submitbtndisable');
                                $('.ajax_loder').remove();
                                if (msg.success == true) {
                                    var newDiv = $(document.createElement('div'));
                                    var htmltag = '<p><span class="ui-icon ui-icon-check" style="float:left;margin-right:10px"></span> ' + msg.responseText + '';
                                    htmltag += '</p>';
                                    newDiv.html(htmltag);
                                    newDiv.dialog({
                                        autoOpen: false,
                                        title: "Information",
                                        height: 150, width: 250, modal: true,
                                        buttons: {
                                            Ok: function (e) {
                                                newDiv.dialog("close");
                                                newDiv.remove();
                                                jQuery(dia).dialog("close");
                                                obj.onsubmitreturnfunction(msg.data);
                                            }
                                        }
                                    });
                                    newDiv.dialog('open');
                                    $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                                } else {
                                    console.log(msg);
                                }

                            });
                            editajaxdata.error(function (xhr, status, error) {
                                $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('enable').removeClass('submitbtndisable');
                                $('.ajax_loder').remove();
                                var newDiv = $(document.createElement('div'));
                                var htmltag = '<p><span class="ui-icon ui-icon-alert" style="float:left;margin-right:10px"></span> ' + xhr.status + '"-"' + xhr.statusText + '';
                                htmltag += '</p>';
                                newDiv.html(htmltag);
                                newDiv.dialog({
                                    autoOpen: false,
                                    title: "Information",
                                    height: 130,
                                    width: 250,
                                    modal: true,
                                    buttons: {
                                        Ok: function () {
                                            newDiv.remove();
                                            newDiv.dialog("close");
                                            jQuery(dia).dialog("close");
                                        }
                                    }
                                });
                                newDiv.dialog('open');
                                $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                            });
                        }
                    },
                    Cancel: function () {
                        jQuery(init).dialog('close');
                    }
                }
            });
            jQuery(init).dialog('open');
            $('.ui-dialog-buttonpane').find('button:contains("Submit")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-disk"></span>');
            $('.ui-dialog-buttonpane').find('button:contains("Cancel")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-closethick"></span>');
        };
        $.fn.AddDataToTextbox = function (fn) {
            var obj = $.extend({
                appendTo: null,
                appendToId: null,
                lookupurl: null,
                NoOfRecordToShow: null,
                setnameofthelookupbyppage: null,
                lookupdiv: null,
                lookuppagename: null,
                pagename: null,
                readonly: null,
            }, fn);
            if (obj.appendTo != null && obj.readonly == true) {
                $(obj.appendTo).keydown(function (e) {
                    if ((e.keyCode === 8 || e.keyCode === 46) || (e.keyCode >= 37 && e.keyCode <= 40)) {
                        return true; // backspace (8) / delete (46)
                    }
                    return false;
                });
            }
            function pageon() {
                //--------------------------------initial---------------------
                var dataontable = obj.NoOfRecordToShow || 5;
                window.pager = new Pager(obj.setnameofthelookupbyppage, dataontable);
                pager.init();
                pager.showPageNav('pager', obj.pagename);
                pager.showPage(1);
                //--------------------------------catch---------------------------
                function Pager(tableName, itemsPerPage) {
                    this.tableName = tableName;
                    this.itemsPerPage = itemsPerPage;
                    this.currentPage = 1;
                    this.pages = 0;
                    this.inited = false;

                    this.showRecords = function (from, to) {
                        var rows = document.getElementById(tableName).rows;
                        // i starts from 1 to skip table header row
                        for (var i = 1; i < rows.length; i++) {
                            if (i < from || i > to)
                                rows[i].style.display = 'none';
                            else
                                rows[i].style.display = '';
                        }
                    }

                    this.init = function () {
                        var rows = document.getElementById(tableName).rows;
                        var records = (rows.length - 1);
                        this.pages = Math.ceil(records / itemsPerPage);
                        this.inited = true;
                    }

                    this.showPageNav = function (pagerName, positionId) {
                        if (!this.inited) {
                            alert("not inited");
                            return;
                        }
                        var element = document.getElementById(positionId);
                        var pagerHtml = '<span onclick="' + pagerName + '.prev();" class="pg-normal"><span style="cursor:pointer">&#x21E6; Prev</span></span> | ';
                        for (var page = 1; page <= this.pages; page++)
                            pagerHtml += '<span id="pg' + page + '" class="pg-normal" onclick="' + pagerName + '.showPage(' + page + ');" style="cursor:pointer;">' + page + '</span> | ';
                        pagerHtml += '<span onclick="' + pagerName + '.next();" class="pg-normal"><span  style="cursor:pointer;">Next &#x21E8;</span></span>';

                        element.innerHTML = pagerHtml;
                    }
                    this.prev = function () {
                        if (this.currentPage > 1)
                            this.showPage(this.currentPage - 1);
                    }

                    this.next = function () {
                        if (this.currentPage < this.pages) {
                            this.showPage(this.currentPage + 1);
                        }
                    }
                    this.showPage = function (pageNumber) {
                        if (!this.inited) {
                            alert("not inited");
                            return;
                        }

                        var oldPageAnchor = document.getElementById('pg' + this.currentPage);
                        oldPageAnchor.className = 'pg-normal';

                        this.currentPage = pageNumber;
                        var newPageAnchor = document.getElementById('pg' + this.currentPage);
                        newPageAnchor.className = 'pg-selected';

                        var from = (pageNumber - 1) * itemsPerPage + 1;
                        var to = from + itemsPerPage - 1;
                        this.showRecords(from, to);
                    }
                }
            };
            var clickon = function () {
                var table = document.getElementById(obj.lookuppagename);
                var tbody = table.getElementsByTagName("tbody")[0];
                tbody.onclick = function (e) {
                    e = e || window.event;
                    var data = [];
                    var target = e.srcElement || e.target;
                    while (target && target.nodeName !== "TR") {
                        target = target.parentNode;
                    }
                    if (target && target.rowIndex > 0) {
                        var cells = target.getElementsByTagName("td");
                        for (var i = 0; i < cells.length; i++) {
                            data.push(cells[i].innerHTML);
                        }
                        if (target.className != "selectedtr") {
                            target.className = "selectedtr";
                            if (obj.appendTo != null) {
                                //console.log($(obj.appendToId));
                                //console.log(data[1]);
                                $(obj.appendToId).val(data[0]);
                                $(obj.appendTo).val(data[1]);
                            }
                        }
                        else {
                            target.className = "";
                            if (obj.appendTo != null) {
                                $(obj.appendToId).val("");
                                $(obj.appendTo).val("");
                            }
                        }
                    }
                }
            };
            var searchon = function () {
                $('#lookup-search').on('keyup', function () {
                    var value = jQuery(this).val().toUpperCase().toLowerCase();
                    var $tablerows = $('#' + obj.setnameofthelookupbyppage + ' tr');
                    if (value === '') {
                        $tablerows.show(500);
                        pageon();
                        $('#' + obj.pagename + '').show();
                        return false
                    }
                    $tablerows.each(function (index) {
                        if (index !== 0) {

                            $tablerows = $(this);
                            var column2 = $tablerows.find("td").eq(1).text().toUpperCase().toLowerCase();
                            if ((column2.indexOf(value) > -1)) {
                                $tablerows.show(500);
                            }
                            else {
                                $tablerows.hide(500);
                            }
                        }
                        $('#' + obj.pagename + '').hide();
                    });
                });
            };
            var lookupajaxdata;
            var init = jQuery(this).dialog({
                autoOpen: false,
                width: 'auto',
                height: 'auto',
                modal: true,
                title: "Lookup Model",
                closeOnEscape: false,
                open: function (event, ui) {
                    $(".ui-dialog-titlebar-close", ui.dialog | ui).on('click', function () {
                        jQuery(init).dialog('destroy').hide();
                    });
                    lookupajaxdata = $.ajax({
                        url: obj.lookupurl,
                        type: 'POST',
                        cache: false,
                        contentType: 'application/json',
                        datatype: 'json',
                    });
                    lookupajaxdata.done(function (successdata) {
                        var htmltag = '<div style=" margin: 0px auto; width: 181px; padding: 1px 0px 5px;"><label >Search : </label><input type="text" id="lookup-search" /></div><table class="lookuptable" id="' + obj.setnameofthelookupbyppage + '"><tr><th>Sr.No</th><th>Description</th></tr>'
                        jQuery.each(successdata, function (i, k) {
                            htmltag += '<tr tabindex="-1">' +
                                '<td>' + k.srno + '</td>' +
                                '<td>' + k.lookupvalue + '</td>' +
                                '</tr>'
                        });
                        htmltag += '</table>';
                        jQuery('.' + obj.lookupdiv + '').html(htmltag);
                        clickon();
                        pageon();
                        searchon();
                    });
                },
                buttons: {
                    Ok: function () {
                        jQuery(this).dialog('destroy').hide();
                    }
                }
            });
            jQuery(this).dialog('open');
        };
        var ReturnStructIds = function (filterid, returnfun) {
            $.ajax({
                method: "Get",
                url: "../Transcation/ByDefaultLoadEmp",
                success: function (data) {
                    console.log(data);
                    if (data.GeoStruct != null) {
                        $('#geo_id').val(data.GeoStruct);
                    }
                    if (data.PayStruct != null) {
                        $('#pay_id').val(data.PayStruct);
                    }
                    if (data.FunStruct != null) {
                        $('#fun_id').val(data.FunStruct);
                    }
                    var data = {
                        GeoStruct: $('#geo_id').val() || null,
                        PayStruct: $('#pay_id').val() || null,
                        FunStruct: $('#fun_id').val() || null,
                        Filter: $(filterid).val() || null,
                    };
                    returnfun(data);
                }
            });
        };
        var ReturnStructIdsV2 = function (returnfun) {
            $.ajax({
                method: "Get",
                url: "../Transcation/ByDefaultLoadEmp",
                success: function (data) {
                    console.log(data);
                    if (data.GeoStruct != null) {
                        $('#geo_id').val(data.GeoStruct);
                    }
                    if (data.PayStruct != null) {
                        $('#pay_id').val(data.PayStruct);
                    }
                    if (data.FunStruct != null) {
                        $('#fun_id').val(data.FunStruct);
                    }
                    var data = {
                        GeoStruct: $('#geo_id').val() || null,
                        PayStruct: $('#pay_id').val() || null,
                        FunStruct: $('#fun_id').val() || null,
                    };
                    returnfun(data);
                }
            });
        };
        $.LoadEmpByDefault = function (single, filterid) {
            single = single || false;
            ReturnStructIds(filterid, function (data) {
                var forwarddata = JSON.stringify(data);
                $('#employee-table').find('td').remove();
                $('#employee-table').P2BTransactionTableDynamic('#employee-search', '#case-employee', 'Transcation/Get_Employelist?geo_id=' + forwarddata + '', "", single);
            });

        };
        $.LoadEmpByDefaultV2 = function () {
            ReturnStructIdsV2(function (data) {
                $('#employee_table').find('td').remove();
                var forwarddata = JSON.stringify(data);
                $('#employee_table').P2BTransactionTableDynamic('#employee-search', '#case-employee', '../Transcation/Get_Emplist?geo_id=' + forwarddata + '', "");
            });
        };
        $.fn.P2BSelectMenuMuliSelectAppend = function (url, forwardata, forwardata2) {
            var init = jQuery(this);
            var w = $(init).css('width');
            var htm = '';
            jQuery(init).empty().append(htm);
            $.post(url, { data: forwardata, data2: forwardata2 }, function (data) {
                $.each(data, function (i, k) {
                    jQuery(init).append($('<option>', {
                        value: k.Value,
                        text: k.Text,
                        selected: k.Selected
                    }));
                });
            });
        };
        $.fn.CustomeCreateDialog = function (fn) {
            var obj = $.extend({
                height: null,
                width: null,
                mode: null,
                title: null,
                submiturl: null,
                form: null,
                gridid: null,
                creaturl: null,
                lookuptableid: null,
                gridreloadname: null,
                submitfun: null,
                btndisableid: null,

            }, fn);
            jQuery(this).trigger('reset');
            var init = jQuery(this);
            var ajaxdata, createajaxdata;
            obj.btndisableid = '.popup-content-icon-edit,.popup-content-icon-remove';
            var maindailog = jQuery(init).dialog({
                iconButtons: [{
                    text: "Help",
                    icon: "ui-icon-help",
                }],
                autoOpen: false,
                height: obj.height,
                width: obj.width,
                modal: true,
                closeOnEscape: false,
                title: obj.title,
                beforeClose: function () {
                    jQuery(init).find('select').empty().append("<option value=0 selected=true>-Select-</option>").selectmenu("refresh");
                    jQuery(init).find('input').empty();
                    jQuery(init).find('textarea').empty();
                    jQuery(init).find(obj.btndisableid).button('enable').removeClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                    jQuery(init).find(obj.lookuptableid).find('tr td').parent().remove();
                },
                open: function (event, ui) {
                    //$('.ui-dialog-titlebar-help').click(function () {
                    //    helpfun("create", "" + obj.form.slice("4") + "");
                    //});
                    //createajaxdata = $.ajax({
                    //    url: obj.creaturl,
                    //    method: 'POST',
                    //    data: { data: creadata }
                    //});
                    //createajaxdata.done(function (value) {

                    //    returnfunctiondata(value);
                    //});
                    jQuery(init).find(obj.btndisableid).button('disable').addClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                    OnpageAlter();
                },
                buttons: {
                    Submit: function () {
                        var x = PerformValidations(obj.form);
                        if (x == true) {
                            ajaxdata = $.ajax({
                                url: obj.submiturl,
                                method: "POST",
                                data: $(obj.form).serialize(),
                                beforeSend: function () {
                                    // alert('hiihihihihi');
                                    $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('disable').addClass('submitbtndisable');
                                    ajaxloaderv2('body');

                                },
                            });
                            ajaxdata.success(function (msg) {
                                $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('enable').removeClass('submitbtndisable');
                                $('.ajax_loder').remove();
                                if (msg.success == true) {
                                    var newDiv = $(document.createElement('div'));
                                    var htmltag = '<p><span class="ui-icon ui-icon-check" style="float:left;margin-right:10px"></span>' + msg.responseText + '';
                                    htmltag += '</p>';
                                    newDiv.html(htmltag);
                                    newDiv.dialog({
                                        autoOpen: false,
                                        title: "Information",
                                        height: 150, width: 250, modal: true,
                                        buttons: {
                                            Ok: function (e) {
                                                if (obj.gridreloadname != '' || obj.gridreloadname == null) {
                                                    jQuery(obj.gridreloadname).trigger('reloadGrid');
                                                }

                                                newDiv.dialog("close");
                                                $(newDiv).remove();
                                                jQuery(maindailog).dialog("close");
                                                if (typeof obj.submitfun === 'function') {
                                                    obj.submitfun(msg);
                                                }
                                            }
                                        }
                                    });
                                } else {
                                    console.log(msg);
                                }

                                $(newDiv).dialog('open');
                                $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                            });
                            ajaxdata.error(function (msg) {
                                $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('enable').removeClass('submitbtndisable');
                                $('.ajax_loder').remove();
                                var newDiv = $(document.createElement('div'));
                                var htmltag = '<p><span class="ui-icon ui-icon-alert" style="float:left;margin-right:10px"></span>' + msg.status + '"-"' + msg.statusText + '';
                                htmltag += '</p>';
                                newDiv.html(htmltag);
                                newDiv.dialog({
                                    autoOpen: false,
                                    title: "Information",
                                    height: 130,
                                    width: 250,
                                    modal: true,
                                    buttons: {
                                        Ok: function () {
                                            if (obj.gridreloadname != '' || obj.gridreloadname == null) {
                                                jQuery(obj.gridreloadname).trigger('reloadGrid');
                                            }
                                            newDiv.dialog("close");
                                            $(newDiv).remove();
                                        }
                                    }
                                });
                                $(newDiv).dialog('open');
                                $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                            });
                        }
                    },
                    Cancel: function () {
                        jQuery(init).dialog("close");
                    }
                }
            });
            jQuery(maindailog).dialog('open');
            $('.ui-dialog-buttonpane').find('button:contains("Submit")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-disk"></span>');
            $('.ui-dialog-buttonpane').find('button:contains("Cancel")').removeClass('ui-buttontext-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-closethick"></span>');
        };
        $.fn.PartialCreateDialog = function (fn) {
            var obj = $.extend({
                height: 'auto',
                width: 'auto',
                form: null,
                title: null,
                htmlurl: null,
                state: "open",
                mode: null,
                editurl: null,
                submiturl: null,
                editdata: null,
                forwarddata: null,
                returndatafunction: null,
                submitfun: null
            }, fn);
            if (obj.htmlurl == null) {
                return false;
            }
            obj.mode = obj.mode.toUpperCase();
            var init = $(this);
            if (obj.mode != "delete") {
                // console.log(this);
                var maindailog = jQuery(this).dialog({
                    autoOpen: false,
                    height: obj.height,
                    width: obj.width,
                    modal: true,
                    closeOnEscape: false,
                    title: obj.title,
                    beforeClose: function () {
                        jQuery(init).find("select").detach();
                        jQuery(init).remove();
                    },
                    open: function (event, ui) {
                        function Appenddata() {

                            var editajaxopenloaddata = $.ajax({
                                url: obj.editurl,
                                method: 'POST',
                                data: { data: obj.editdata },
                            });
                            editajaxopenloaddata.done(function (value) {
                                if (typeof obj.returndatafunction === "function" && obj.mode != "delete") {
                                    obj.returndatafunction(value);
                                }
                                if (obj.mode.toUpperCase() == "VIEW") {
                                    //console.log(init);
                                    $(init).find('input').attr('readonly', 'readonly');
                                    $(obj.form).find('button').button('disable');

                                }
                            });
                        };
                        jQuery.ajax({
                            url: obj.htmlurl,
                            method: "GET",
                            beforeSend: function () {
                                ajaxloader(init);
                                $(init).find("div .ajax_loder").show();
                            },
                            success: function (result, status, xhr) {
                                showajaxeroor(xhr, init);
                                $(init).find("div .ajax_loder").hide();
                                $(init).html(result);
                                if (obj.mode != null && obj.mode != "delete" && obj.editurl != null && obj.editdata != null) {

                                    Appenddata();
                                }
                            },
                            error: function (xhr, status, error) {
                                showajaxeroor(xhr, init);
                                $(init).find("div .ajax_loder").show();
                            },
                            complete: function () {
                                $(init).find("div .ajax_loder").hide();
                            }
                        });
                    },
                    buttons: {
                        Submit: function () {
                            if (obj.mode.toUpperCase() == "VIEW") {
                                $(maindailog).dialog('close');
                                $(maindailog).remove();
                                if (typeof obj.submitfun === "function") {
                                    obj.submitfun('msg');
                                }
                            } else {
                                debugger
                                var x = PerformValidations(obj.form);
                                if (x == true) {
                                    var trainingdata = "";
                                    if (obj.form.toUpperCase() == "#FRMTRAININGNEEDREQUEST" || obj.form.toUpperCase() == "#TRAININGNEEDREQPARTIAL" || obj.form.toUpperCase() == "#FRMTRAININGPRESENTY" || obj.form.toUpperCase() == "#FRMTRAININGFEEDBACKREQUEST"
                                        || obj.form.toUpperCase() == "#FRMAPPRAISALNEEDREQUEST") {
                                        trainingdata = $.LocalStorageHelper.Get("FormTrainingNeedData")
                                    }
                                    var ajaxdata = $.ajax({
                                        url: obj.submiturl,
                                        method: "POST",
                                        beforeSend: function () {
                                            $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('disable').addClass('submitbtndisable');
                                            ajaxloaderv2('body');
                                        },
                                        data: $(obj.form).serialize() + '&data=' + obj.forwarddata + '&TrainingData=' + trainingdata,
                                    });
                                    ajaxdata.done(function (msg) {
                                        $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('enable').removeClass('submitbtndisable');
                                        $('.ajax_loder').remove();

                                        if (msg.status == true) {
                                            msg.valid = msg.valid || false;
                                            var newDiv = $(document.createElement('div'));
                                            var htmltag = '<p><span class="ui-icon ui-icon-check" style="float:left;margin-right:10px"></span> ' + msg.responseText + '';
                                            htmltag += '</p>';
                                            newDiv.html(htmltag);
                                            newDiv.dialog({
                                                autoOpen: false,
                                                title: "Information",
                                                closeOnEscape: false,
                                                height: 150, width: 250, modal: true,
                                                buttons: {
                                                    Ok: function () {
                                                        newDiv.dialog("close");
                                                        newDiv.remove();
                                                        if (msg.valid == false) {
                                                            jQuery(maindailog).dialog("close");
                                                            jQuery(init).remove();
                                                            if (typeof obj.submitfun === "function") {
                                                                obj.submitfun(msg);
                                                            }
                                                        } else {
                                                            //  alert(msg.responseText);
                                                        }
                                                    }
                                                }
                                            });
                                            newDiv.dialog('open');

                                        } else {
                                            var newDiv = $(document.createElement('div'));
                                            var htmltag = '<p><span class="ui-icon ui-icon-check" style="float:left;margin-right:10px"></span>Message :' + msg.responseText + '';
                                            htmltag += '</p>';
                                            newDiv.html(htmltag);
                                            newDiv.dialog({
                                                autoOpen: false,
                                                title: "Information",
                                                closeOnEscape: false,
                                                height: 150, width: 250, modal: true,
                                                buttons: {
                                                    Ok: function () {
                                                        newDiv.dialog("close");
                                                        $(newDiv).remove();
                                                        // maindailog.dialog("close");
                                                        // maindailog.remove();
                                                    }
                                                }
                                            });
                                            newDiv.dialog('open');
                                        }
                                    });
                                    ajaxdata.fail(function (jqXHR, textStatus) {
                                        $('.ui-dialog-buttonpane').find('button:contains("Submit")').button('enable').removeClass('submitbtndisable');
                                        $('.ajax_loder').remove();
                                        var newDiv = $(document.createElement('div'));
                                        var htmltag = '<p><span class="ui-icon ui-icon-alert" style="float:left;margin-right:10px"></span> ' + jqXHR.status + '"-"' + jqXHR.statusText + '';
                                        htmltag += '</p>';
                                        newDiv.html(htmltag);
                                        newDiv.dialog({
                                            autoOpen: false,
                                            title: "Information", closeOnEscape: false,
                                            height: 130, width: 250, modal: true,
                                            buttons: {
                                                Ok: function () {
                                                    newDiv.dialog("close");
                                                }
                                            }
                                        });
                                        newDiv.dialog('open');
                                        $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                                    });
                                }
                            }
                        },
                        Cancel: function () {
                            jQuery(init).dialog("close");
                        }
                    },
                    Cancel: function () {
                        jQuery(init).dialog("close");
                    }
                });
                jQuery(maindailog).dialog(obj.state);
            } else {
                var deleteajaxdata;
                deletedata, forwarddata = 0;
                var newDiv = $(document.createElement('div'));
                var htmltag = '<p><span class="ui-icon ui-icon-alert" style="float:left;margin-right:10px"></span> ' + deletemessage + '';
                htmltag += '</p>';
                newDiv.html(htmltag);
                newDiv.dialog({
                    autoOpen: false,
                    title: "Delete Confirmation !",
                    height: height,
                    width: width,
                    modal: true,
                    beforeClose: function () {
                        newDiv.remove();
                    },
                    buttons: {
                        Confirm: function () {
                            var newDivdelete = $(document.createElement('div'));
                            var htmltag = '<p><span class="ui-icon ui-icon-check" style="float:left;margin-right:10px"></span> Record Removed';
                            htmltag += '</p>';
                            newDivdelete.html(htmltag);
                            newDivdelete.dialog({
                                closeOnEscape: false,
                                dialogClass: "no-close",
                                autoOpen: false,
                                title: "Information",
                                height: "auto",
                                width: 425,
                                modal: true,
                                buttons: {
                                    Ok: function (e) {
                                        if (typeof obj.submitfun === "function") {
                                            obj.submitfun("Record Remove");
                                        }
                                    }
                                }
                            });
                            newDivdelete.dialog('open');
                        },
                        Cancel: function () {
                            jQuery(newDiv).dialog("close");
                        }
                    }
                });
                jQuery(newDiv).dialog('open');
            }

        };
        $.fn.OnClickFormReset = function (FormName, emp, fun) {
            $(this).on('change', 'input:checkbox:gt(0)', function (e) {
                var formname = $(FormName);
                formname.trigger('reset');
                formname.find('select').empty().append('<option></option>').selectmenu('refresh');
                formname.find('table').find('tr td').parent().remove();
                $(emp).val($(this).val());
                if (typeof fun === 'function') {
                    return fun();
                }
            });
        };
        $.FormReset = function (form) {
            var formname = $(form);
            formname.trigger('reset');
            formname.find('select').empty().append('<option></option>').selectmenu('refresh');
            formname.find('table').find('tr td').parent().remove();
        }
        $.fn.TodayDate = function () {
            var d = new Date();
            var dd = parseInt(d.getDate());
            var mm = parseInt(d.getMonth() + 1);
            var yy = parseInt(d.getFullYear());
            if (dd < 10) {
                dd = '0' + dd;
            } if (mm < 10) {
                mm = '0' + mm;
            }
            $(this).val(dd + "/" + mm + "/" + yy);
        };
        $.fn.P2bViewPartialDialog = function (fn) {

            var diatitle = fn.btnid.split('-')[1],
                formid = "#frm" + fn.btnid.split('-')[2];
            var obj = $.extend({
                title: diatitle,
                btnid: null,
                formid: formid,
                htmurl: null,
                editurl: null,
                height: 'auto',
                width: 'auto',
                returndatafunction: null,
                editdata: null,
            }, fn);
            if (obj.editdata == null) {
                return false;
            }
            var init = $(this);
            var maindailog = jQuery(this).dialog({
                autoOpen: false,
                height: obj.height,
                width: obj.width,
                modal: true,
                closeOnEscape: false,
                title: obj.title,
                beforeClose: function () {
                    jQuery(init).find("select").detach();
                    jQuery(init).remove();
                },
                open: function (event, ui) {
                    function Appenddata() {
                        var editajaxopenloaddata = $.ajax({
                            url: obj.editurl,
                            method: 'POST',
                            data: { data: obj.editdata },
                        });
                        editajaxopenloaddata.done(function (value) {
                            if (typeof obj.returndatafunction === "function") {
                                obj.returndatafunction(value);
                            }
                            //if (obj.mode.toUpperCase() == "VIEW") {
                            $(init).find('input').attr('readonly', 'readonly');
                            $(init).find('button').button('disable');
                            $(init).find('button').buttonset("option", "disabled", true);


                            //}
                        });
                    };
                    jQuery.ajax({
                        url: obj.htmurl,
                        method: "GET",
                        beforeSend: function () {
                            ajaxloader(init);
                            $(init).find("div .ajax_loder").show();
                        },
                        success: function (result, status, xhr) {
                            showajaxeroor(xhr, init);
                            $(init).find("div .ajax_loder").hide();
                            $(init).html(result);
                            Appenddata();
                        },
                        error: function (xhr, status, error) {
                            showajaxeroor(xhr, init);
                            $(init).find("div .ajax_loder").show();
                        },
                        complete: function () {
                            $(init).find("div .ajax_loder").hide();
                        }
                    });
                },
                buttons: {
                    Cancel: function () {
                        jQuery(init).dialog("close");
                    }
                }
            });
            jQuery(maindailog).dialog("open");
        };
        $.fn.SelectMenuOnChange = function (url, filter_drop, data2, data3, callbck) {
            var init = jQuery(this);
            jQuery(init).off("selectmenuchange").on("selectmenuchange", function () {
                jQuery(filter_drop).empty().append("<option value=0 selected=true>-Select-</option>").selectmenu('refresh');
                var value = jQuery(init).val();
                if (value != 0) {
                    $.post(url, { data: value, data2: data2, data3: data3 }, function (data) {
                        $.each(data, function (i, k) {
                            jQuery(filter_drop).append($('<option>', {
                                value: k.Value,
                                text: k.Text,
                                selected: k.Selected
                            }));
                        });
                        jQuery(filter_drop).selectmenu("refresh").selectmenu("menuWidget").css({ "height": "auto" });
                        if (data != null) {
                            if (typeof callbck === "function") {
                                callbck();
                            }
                        }

                    });
                } else {
                    if (typeof callbck === "function") {
                        callbck();
                    }
                    jQuery(filter_drop).empty()
                    .append("<option value=0 selected=true>-Select-</option>")
                    .css({ "height": "auto" });
                }
            });
        };
        $.fn.SelectMenuAppend = function (url, forwardata, forwardata2, drop2) {
            var init = jQuery(this);
            var w = $(init).css('width');
            var htm = '<option style=' + w + ' value=0 selected=true>-Select-</option>';
            jQuery(init).empty().append(htm).selectmenu("refresh");
            $.post(url, { data2: forwardata, data3: forwardata2 }, function (data) {
                $.each(data, function (i, k) {
                    jQuery(init).append($('<option>', {
                        value: k.Value,
                        text: k.Text,
                        selected: k.Selected
                    }));
                });
                jQuery(init).selectmenu('refresh').selectmenu("menuWidget").css("height", "100px");
            });
            jQuery(drop2).empty().append(htm).selectmenu("refresh");
        };
        $.fn.P2BTransactionTable = function (IDofinputsearch, IDofCheckbox, urldatatoload, forwardata, alterurl) {

            var init = $(this), alterurl = alterurl || false;
            var datatoserverside = [];
            if (forwardata == '' || forwardata == null) {
                datatoserverside = ["9999999999"];
            }
            else {
                datatoserverside = forwardata
            }
            var searchon = function () {
                jQuery(IDofinputsearch).on('keyup', function (e) {
                    var value = jQuery(this).val().toUpperCase().toLowerCase();
                    var $rows = $('#' + init.attr('id') + ' tr');
                    if (value == '') {
                        $rows.show(500);
                        return false;
                    }
                    $rows.each(function (index) {
                        if (index !== 0) {
                            var $row = $(this);
                            var column2 = $row.find("td").eq(2).text().toUpperCase().toLowerCase();
                            if ((column2.indexOf
                                (value) > -1)) {
                                $row.show(500);
                            }
                            else {
                                $row.hide(500);
                            }
                        }
                    });
                });
            };
            var RemoveElement = function () {
                $(init).on("click", "tr td span.transcation_delete", function (e) {
                    $(this).parents('tr').remove();
                });
            }
            function checkboselection() {

                $(IDofCheckbox).on('click', function (e) {
                    var b = init.attr('id');
                    var count = $("#" + b + " tr").filter(function () {
                        return $(this).css('display') == 'none';
                    }).length;
                    if (this.checked) {
                        $('#' + b + ' .case').each(function () {
                            if (count > 0) {
                                var data = $("#" + b + " tr").filter(function () {
                                    return $(this).css('display') == 'none';
                                }).find('.case').removeAttr('checked').checked = false;
                                var data_1 = $("#" + b + " tr:not(:first-child)").filter(function () {
                                    return $(this).css('display') !== 'none';
                                }).addClass('selectedtr').find('.case').attr('checked', 'checked').prop('checked', true).checked = true;
                                if (this.checked) {
                                    var abcdert = jQuery(this).val();
                                    console.log(value);

                                }
                            }
                            else {
                                $('#' + b + ' .case').attr('checked', 'checked');
                                this.checked = true;
                                var value_checked = jQuery('#' + b + ' .case:checked').parent('td').parent('tr');
                                value_checked.addClass('selectedtr');
                                if (this.checked) {
                                    var abcd = jQuery(this).val();
                                }
                                else {
                                    alert('No Data Found');
                                }
                            }
                        });
                    } else {
                        $('#' + b + ' .case').each(function (e) {
                            var value_unchecked = jQuery(this).parent('td').parent('tr');
                            value_unchecked.removeClass('selectedtr');
                            $('#' + b + ' .case').removeAttr('checked');
                            this.checked = false;
                        });
                    }
                });
                //
                $(document).on('click', '#' + init.attr('id') + ' .case', function (e) {
                    if (this.checked) {
                        var d = jQuery(this).val();
                        var value_checked = jQuery('.case:checked').parent('td').parent('tr');
                        value_checked.addClass('selectedtr');
                    }
                    else {
                        var value_unchecked = jQuery(this).parent('td').parent('tr');
                        value_unchecked.removeClass('selectedtr');
                    }
                    if ($('.case:checked').length == $('.case').length) {
                        $(IDofCheckbox).prop('checked', true);
                    } else {
                        $(IDofCheckbox).prop('checked', false);
                    }
                });
            };
            var dataload;
            if (alterurl == true) {
                dataload = jQuery.ajax({
                    url: urldatatoload,
                    type: 'POST',
                    data: { data2: forwardata[0].toString(), data3: forwardata[1].toString() }
                });
            } else {

                dataload = jQuery.ajax({
                    url: urldatatoload,
                    type: 'POST',
                    contentType: 'application/json',
                    cache: false,
                    data: JSON.stringify({ data: datatoserverside })
                });
            }
            dataload.done(function (returndata) {
                $(init).find('tr td').remove();
                $.each(returndata, function (i, k) {
                    jQuery(init).append('<tr tabindex="1"><td><input type="checkbox" class="case" name=' + $(init).attr('id') + ' value=' + k.code + ' /></td><td style="display:none;">' + k.code + '</td><td>' + k.value + '<span title="Delete" class="transcation_delete" style="float:right">&#9932;</span></td></tr>').insertAfter(jQuery(init).closest('tr'));
                });
                searchon();
                checkboselection();
                RemoveElement();
            });

        };
        var JqGridCheck = {
            select_all: false,
            data: null,
            Get: function () {
                return this.data != null ? this.data.toString() : null;
            },
            Set: function (data) {
                if (data != null) {
                    this.data = data.toString();
                }
            },
            Del: function () {
                this.select_all = false;
                this.data = null;
            }
        };

        /*
            Ess Script
        */
        $.fn.AppendLink = function (dataurl) {
            if (dataurl != undefined) {
                var id = $(this).attr('id');

                var ajx = $.ajax({
                    url: dataurl,
                    beforeSend: function () {
                        CardAjaxLoder("#" + id + "");
                    },
                    complete: function () {
                        $('#' + id + '').find('.CardAjaxLoaderClass').remove();
                    },
                    success: function (data) {
                        if (data.status == true) {
                            $.each(data.data, function (i, k) {
                                console.log(k);
                                $('#' + id + ' tr:last').after('<tr class="new-lvnewreq-span" ><td id="newtd" data-id="' + k.id + '">' + k.val + '</td></tr>');
                            });
                        } else {
                            console.log(data);
                        }
                    }
                });
            }
        };
        $.fn.AppendLinkWithAction = function (fn) {
            var obj = $.extend({
                dataurl: null,
                classname: null,
                showrejction: false,
                showHide: null
            }, fn);


            var id = $(this).attr('id');
            var parent = $(this).closest('div.dashboard-card');
            //console.log(parent);
            if (obj.dataurl == "/ITInvestmentPayment/GetMyItInvestment") {
                // parent.css({ "width": "91.3%" });
            } else {
                //parent.css({ "width": "45%" });
            }
            // $(this).css({ "height": "195px" });

            /*
            dataurl: url to get data,
            classname: append this class name to tr,after words this class name bind with context 			menu ,
             showrejction: change colour on rejection ,
            */
            $('#' + id + ' tr').remove();
            var ajx = $.ajax({
                url: obj.dataurl,
                beforeSend: function () {
                    CardAjaxLoder("#" + id + "");
                },
                complete: function () {
                    $('#' + id + '').find('.CardAjaxLoaderClass').remove();
                },
                success: function (data) {
                    if (data.status == true) {
                        if (obj.showHide == true) {
                            $("#" + id + "").show();
                        }
                        //$.each(data.data, function (i, k) {
                        //    var classname;
                        //    if (k.status == "0" && obj.showrejction == true) {
                        //        classname = obj.classname + " btn btn-neutral td_contentmargine rejectlv";
                        //    } else {
                        //        classname = obj.classname + " btn btn-neutral td_contentmargine";
                        //    }
                        //    $('#' + id + ' tr:last').after('<tr class="new-lvnewreq-span" ><td id="newtd"  data-ItinvestmentId="' + k.ItinvestmentId + '" data-LvNewReq="' + k.LvNewReq + '" data-EmpLVid="' + k.EmpLVid + '" data-LvHead_Id="' + k.LvHead_Id + '" data-EmpId="' + k.EmpId + '" data-status="' + k.status + '"class="' + classname + '">' + k.val + '</td></tr>');
                        //});
                        var count = 0;
                        var temp = "<thead><tr>";
                        var availableTags = [];
                        $.each(data.data, function (i, k) {
                            if (i == 0) {
                                $.each(k, function (l, j) {
                                    if (l != "RowData" && j != null) {
                                        temp += "<th>" + j + "</th>";
                                    }
                                });
                                temp += "</tr></thead>";
                            } else {
                                count++;
                                //$('#' + id + ' tr:last').after('<tr class="new-lvnewreq-span" ><td id="newtd"  data-ItinvestmentId="' + k.ItinvestmentId +
                                //    '" data-LvNewReq="' + k.LvNewReq + '" data-EmpLVid="' + k.EmpLVid + '" data-LvHead_Id="' + k.LvHead_Id + '" data-EmpId="' +
                                // k.EmpId + '" data-status="' + k.status + '"class="' + classname + '">' + k.val + '</td></tr>');

                                temp += '<tr id="newtd" class="new-lvnewreq-span3 ' + obj.classname + '"  data-ItinvestmentId="' + k.RowData.ItinvestmentId + '" data-emppayrollid="' +
                                    k.RowData.emppayrollid + '" data-LvNewReq="' + k.RowData.LvNewReq + '" data-EmpLVid="' + k.RowData.EmpLVid +
                                    '" data-LvHead_Id="' + k.RowData.LvHead_Id + '" data-EmpId="' + k.RowData.EmpId + '" data-status="' + k.RowData.Status + '" data-IsClose="' + k.RowData.IsClose + '">';
                                $.each(k, function (l, j) {
                                    if (l != "RowData" && j != null) {
                                        temp += '<td>' + j + '</td>';
                                        availableTags.push(j);
                                    }
                                });
                                temp += "</tr>";
                            }
                        });
                        if (count > 0) {
                            //console.log(temp);
                            $('#' + id + '').append(temp);
                        }
                        $('#' + id + '').find('.CardAjaxLoaderClass').remove();
                        var result = [];
                        availableTags.forEach(function (item) {
                            if (result.indexOf(item) < 0) {
                                if (item != null) {
                                    result.push(item);
                                }
                            }
                        });
                        $("#table-header-search-input").autocomplete({
                            source: result
                        });
                        if ($('#' + id + '').prev('.table-wrapper').length == 0) {
                            $('#' + id + '').parent('.card-body').prev('.card-heading')
                                .append('<div id="expand-hide-div-id_' + id + '" title="Show Filter" class="btn-header expand-hide-div fa fa-fw fa-search card-header-action-btn"></div>');
                            $('#' + id + '').before("<div class='table-wrapper table-tr-hide'><span class='table-header-info'><span class='table-header-search'>" +
                             "<input type='text' class='table-header-search-input content_margine' placeholder='Search..' id='table-header-search-input_" + id + "'></span>" +
                             "</span></div>");
                            $('#' + id + '').after("<span class='table-header-total-row-count content_margine'>Total Rows : " + count + "</span>");
                            $('#expand-hide-div-id_' + id + '').on('click', function () {
                                $('#' + id + '').prev('div.table-wrapper').toggleClass('table-tr-hide');
                            });
                            $('#table-header-search-input_' + id + '').on('keyup', function (e) {
                                if (e.which == 13) {
                                    if ($(this).val()) {
                                        var value = $(this).val().toLowerCase();
                                        $("#" + id + " tr:gt(1)").filter(function (i, k) {
                                            if ($(k).text().toLowerCase().indexOf(value) > -1) {
                                                $(k).removeClass("table-tr-hide");
                                            }
                                            else {
                                                $(k).addClass('table-tr-hide');

                                            }
                                        });
                                    }
                                } else if (e.keyCode == 8) {
                                    $('#' + id + ' tr').removeClass("table-tr-hide");
                                }
                            });
                        }
                    } else {
                        console.log(data);
                        if (obj.showHide == true) {
                            $("#" + id + "").hide();
                        }
                    }
                }
            });
        };
        var CardAjaxLoder = function (comeingfrom) {
            if (comeingfrom == undefined) {
                return false;
            }
            var loc = $(comeingfrom).parents('div.dashboard-card').offset();
            var w = parseInt($(comeingfrom).parents('div.dashboard-card').css('width'));
            if (loc != undefined) {
                var elementleft = parseInt(loc.left);
                $('<div class="CardAjaxLoaderClass"></div>')
                    .appendTo("" + comeingfrom + "");
            }
        };
        $.fn.DetailsControl = function (fn) {
            /*
            usage
            • costume html plug in create for show employee leave balance
            */
            var obj = $.extend({
                dataurl: null,
            }, fn);
            var a = "";
            if (obj.dataurl != null) {
                var id = $(this).attr('id');
                $('#' + id + ' details:gt(0)').remove();
                $(this).append("<details class='detailsclass'><summary></summary></details>");
                var ajx = $.ajax({
                    url: obj.dataurl,
                    beforeSend: function () {
                        CardAjaxLoder("#" + id + "");
                    },
                    complete: function () {
                        $('#' + id + '').find('.CardAjaxLoaderClass').remove();
                    },
                    success: function (data) {
                        if (data.status == true) {
                            console.log(data);
                            $.each(data.Data, function (i, k) {
                                var htm = "";
                                htm += "<details class='custom-marker1 detailsclass'><summary>" + k.EmpName + "</summary>";
                                $.each(k.LvHeadName, function (i, l) {
                                    htm += "<details><summary class='details_margine'>" + l.LvHeadName +
                                        "</summary><span class='summary_bx span'><span class='summary_heading'>Lv Balance Summary</span><span style='padding: 2px 9px;'>" + l.LvHeadBal + "</span></span>";
                                    var fulldetails = "<span class='summary_bx'><span class='summary_heading'>Requisition</span>";
                                    $.each(l.LvReq, function (i, m) {
                                        fulldetails += "<span class='summary_content'>" + m + "</span>";
                                    });
                                    htm += fulldetails + "</details>";
                                });
                                htm += "</details>";
                                $('#' + id + '').append(htm);
                            });
                        } else {
                            $('#' + id + ' details:gt(0)').remove();
                            console.log(data);
                        }
                    }
                });
            } else {
                throw "dataurl null";
            }
        };
        $.fn.DetailsEmpInvestment = function (fn) {
            /*
            usage
            • costume html plug in create for show employee leave balance
            */
            var obj = $.extend({
                dataurl: null,
            }, fn);
            var a = "";
            if (obj.dataurl != null) {
                var id = $(this).attr('id');
                $('#' + id + ' details:gt(0)').remove();
                $(this).append("<details class='detailsclass'><summary></summary></details>");
                var ajx = $.ajax({
                    url: obj.dataurl,
                    beforeSend: function () {
                        CardAjaxLoder("#" + id + "");
                    },
                    complete: function () {
                        $('#' + id + '').find('.CardAjaxLoaderClass').remove();
                    },
                    success: function (data) {
                        if (data.status == true) {

                            console.log(data);
                            $.each(data.Data, function (i, k) {

                                var htm = "";
                                htm += "<details class='custom-marker1 detailsclass'><summary>" + k.EmpName + "</summary>";
                                $.each(k.LvHeadName, function (i, l) {
                                    htm += "<details class='custom-marker2'><summary class='details_margine'>" + l.LvHeadName +
                                        "</summary><span class='summary_bx span GetEmpItInvestmentPayment-linkaction' data-EmpLVid=" + l.LvHeadCode + "><span class='summary_heading'>Summary</span><span style='padding: 2px 9px;'>" + l.LvHeadBal + "</span></span></details>";
                                    //var fulldetails = "<span class='summary_bx'><span class='summary_heading'>Requisition</span>";
                                    //$.each(l.LvReq, function (i, m) {
                                    //    fulldetails += "<span class='summary_content'>" + m + "</span>";
                                    //});
                                    //htm += fulldetails + "</details>";
                                });
                                htm += "</details>";
                                $('#' + id + '').append(htm);
                            });
                        } else {
                            $('#' + id + ' details:gt(0)').remove();
                            console.log(data);
                        }
                    }
                });
            } else {
                throw "dataurl null";
            }
        };

        $.fn.DetailsControlEpmsS = function (fn) {
            /*
            usage
            • costume html plug in create for show employee leave balance
            */
            var obj = $.extend({
                dataurl: null,
            }, fn);
            var a = "";
            if (obj.dataurl != null) {
                var id = $(this).attr('id');
                $('#' + id + ' details:gt(0)').remove();
                $(this).append("<details class='detailsclass'><summary></summary></details>");
                var ajx = $.ajax({
                    url: obj.dataurl,
                    beforeSend: function () {
                        CardAjaxLoder("#" + id + "");
                    },
                    complete: function () {
                        $('#' + id + '').find('.CardAjaxLoaderClass').remove();
                    },
                    success: function (data) {
                        if (data.status == true) {
                            console.log(data);
                            $.each(data.Data, function (i, k) {

                                var htm = "";
                                htm += "<details class='custom-marker1 detailsclass'><summary>" + k.EmpName + "</summary>";
                                $.each(k.LvHeadName, function (i, l) {
                                    htm += "<details class='custom-marker2'><summary class='details_margine'>" + l.LvHeadName +
                                        "</summary><span class='summary_bx span'><span class='summary_heading'>Summary</span><span style='padding: 2px 9px;'>" + l.LvHeadBal + "</span></span></details>";
                                    //var fulldetails = "<span class='summary_bx'><span class='summary_heading'>Requisition</span>";
                                    //$.each(l.LvReq, function (i, m) {
                                    //    fulldetails += "<span class='summary_content'>" + m + "</span>";
                                    //});
                                    //htm += fulldetails + "</details>";
                                });
                                htm += "</details>";
                                $('#' + id + '').append(htm);
                            });
                        } else {
                            $('#' + id + ' details:gt(0)').remove();
                            console.log(data);
                        }
                    }
                });
            } else {
                throw "dataurl null";
            }
        };
        $.fn.DetailsControlEpmsOutdoorRequisitionS = function (fn) {
            /*
            usage
            • costume html plug in create for show employee leave balance
            */
            var obj = $.extend({
                dataurl: null,
            }, fn);
            var a = "";
            if (obj.dataurl != null) {
                var id = $(this).attr('id');
                $('#' + id + ' details:gt(0)').remove();
                $(this).append("<details class='detailsclass'><summary></summary></details>");
                var ajx = $.ajax({
                    url: obj.dataurl,
                    beforeSend: function () {
                        CardAjaxLoder("#" + id + "");
                    },
                    complete: function () {
                        $('#' + id + '').find('.CardAjaxLoaderClass').remove();
                    },
                    success: function (data) {
                        if (data.status == true) {
                            debugger;
                            console.log(data);
                            $.each(data.Data, function (i, k) {
                                var htm = "";
                                htm += "<details class='custom-marker1 detailsclass'><summary>" + k.EmpName + "</summary>";
                                $.each(k.LvHeadName, function (i, l) {
                                    //htm += "<details class='custom-marker2'><summary class='details_margine'>" + l.LvHeadName +
                                    //    "</summary><span class='summary_bx span'><span class='summary_heading'>Summary</span><span style='padding: 2px 9px;'>" + l.LvHeadBal + "</span></span></details>";
                                    var fulldetails = "<span class='summary_bx'><span class='summary_heading'>Requisition</span>";
                                    $.each(l.LvReq, function (i, m) {
                                        fulldetails += "<span class='summary_content'>" + m + "</span>";
                                    });
                                    htm += fulldetails + "</details>";
                                });
                                htm += "</details>";
                                $('#' + id + '').append(htm);
                            });
                        } else {
                            $('#' + id + ' details:gt(0)').remove();
                            console.log(data);
                        }
                    }
                });
            } else {
                throw "dataurl null";
            }
        };
        $.fn.PayslipLink = function (dataurl, dataa, fromdate, todate) {
            if (dataurl != undefined) {
                var id = $(this).attr('id');
                var parent = $(this).closest('div.dashboard-card');
                console.log(id);
                // parent.css({ "width": "45%" });
                $(this).css({ "height": "195px" });
                $('#' + id + ' tr:gt(0)').remove();

                //var empp=localStorage.getItem("payslipemp");
                var ajx = $.ajax({
                    url: dataurl,
                    type: 'post',
                    data: { data: dataa, data1: fromdate, data2: todate },
                    beforeSend: function () {
                        CardAjaxLoder("#" + id + "");
                    },
                    complete: function () {
                        $('#' + id + '').find('.CardAjaxLoaderClass').remove();
                    },
                    success: function (data) {
                        if (data.status == true) {
                            $.each(data.data, function (i, k) {
                                $('#' + id + ' tr:last').after('<tr class="new-payslip-span new-lvnewreq-span" ><td class="' + id + '-td" data-id="' + k.Id + '"data-empid="' + k.EmpId + '">' + k.val + "  " + k.Empname + '</td></tr>');
                            });
                        } else {
                            console.log(data);
                        }
                    }
                });
            }
        };
        $.fn.MiniMaxDiv = function () {
            var init = $(this);
            $(init).on('click', function (e) {
                init = e.target;
                var parent_div = $(init).parent('div').next('.card-body');
                if ($(init).hasClass('fa-minus') && !parent_div.hasClass('hideshowdiv')) {

                    $(init).attr('title', 'Maximise');
                    $(init).removeClass('fa-minus').addClass('fa-plus');
                    parent_div.addClass('hideshowdiv');

                } else if ($(init).hasClass('fa-plus') && parent_div.hasClass('hideshowdiv')) {

                    $(init).attr('title', 'Minimise');
                    $(init).removeClass('fa-plus').addClass('fa-minus');
                    parent_div.removeClass('hideshowdiv');
                }
            });
        };
        $.fn.LvBalance = function (fn) {
            var obj = $.extend({
                dataurl: null,

            }, fn);
            var a = "";
            if (obj.dataurl != null) {
                var id = $(this).attr('id');
                $('#' + id + ' details:gt(0)').remove();
                $(this).append("<details class='detailsclass'><summary></summary></details>");
                var ajx = $.ajax({
                    url: obj.dataurl,
                    beforeSend: function () {
                        CardAjaxLoder("#" + id + "");
                    },
                    complete: function () {
                        $('#' + id + '').find('.CardAjaxLoaderClass').remove();
                    },
                    success: function (data) {
                        if (data.status == true) {
                            $.each(data.Data, function (i, k) {
                                var htm = "";
                                // htm += "<details class='custom-marker1 detailsclass'><summary>" + k.EmpName + "</summary>";
                                $.each(k.LvHeadName, function (i, l) {
                                    htm += "<details class='custom-marker2'><summary class='details_margine'>" + l.LvHeadName +
                                        "</summary><span class='summary_bx span '><span class='summary_heading'>Lv Balance Summary</span><span style='padding: 2px 9px;'>" + l.LvHeadBal + "</span></span>";
                                    var fulldetails = "<span class='summary_bx '><span class='summary_heading'>Requisition</span>";
                                    $.each(l.LvReq, function (i, m) {
                                        fulldetails += "<span class='summary_content'>" + m + "</span>";
                                    });
                                    htm += fulldetails + "</details>";
                                });
                                //   htm += "</details>";
                                $('#' + id + '').append(htm);
                            });
                        } else {
                            $('#' + id + ' details:gt(0)').remove();
                            console.log(data);
                        }
                    }
                });
            } else {
                throw "dataurl null";
            }
        };
        $.Snackbar = function (fn) {
            if (!fn)
                return false;
            var obj = $.extend({
                msg: null,
                time: null,
                setTimeout: true,
            }, fn);
            if (obj.msg != null) {
                var htm = $("<span id='snackbar'>" + obj.msg + "<span>").on('click', function () {
                    $('#snackbar').addClass('show');
                });
                $(htm).appendTo('body');
                $('#snackbar').trigger('click');
                //  console.log(obj.setTimeout);
                if (obj.setTimeout == true)
                    setTimeout(function () { $('#snackbar').removeClass('show'); }, 3000);
            }
        };
        $.fn.P2BSelectMenuAppend = function (url, forwardata, forwardata2, drop2) {
            debugger;
            var init = jQuery(this);
            var w = $(init).css('width');
            var htm = '<option style=' + w + ' value=0 selected=true>-Select-</option>';
            jQuery(init).empty().append(htm).selectmenu("refresh");
            $.post(url, { data: forwardata, data2: forwardata2 }, function (data) {
                $.each(data, function (i, k) {
                    jQuery(init).append($('<option>', {
                        value: k.Value,
                        text: k.Text,
                        selected: k.Selected,
                        data_flg: k.Data
                    }));
                });
                jQuery(init).selectmenu('refresh').selectmenu("menuWidget").css("height", "100px");
            });
            jQuery(drop2).empty().append(htm).selectmenu("refresh");
        };
        $.fn.LoadImg = function (url, data) {
            var id = "#" + $(this).attr('id');
            $.ajax({
                type: 'GET',
                url: url,
                dataType: "json",
                cache: false,
                data: { data: data },
                success: function (data) {

                    if (data != null) {

                        $(id).attr('src', "data:image/png;base64," + data + "");
                    } else {
                        console.log("Error while loading captcha image");
                    }
                },
                error: function (data) {
                }
            });
        };
        $.fn.LinkPopup2 = function (fn) {
            var obj = $.extend({
                init: '#' + $(this).attr('id') + '',
                dataurl: null,
                htmurl: null,
                editurl: null,
                title: null,
                form: null,
                submiturl: null,
                mode: null,
                returndatafunction: null,
                submitfun: null
            }, fn);

            var id, $this = $(this);
            var parent = $(this).closest('div.dashboard-card');
            //  console.log(parent);
            // parent.css({ "width": "45%" })
            $(this).css({ "height": "195px" });
            var init = function (data) {
                /*
                *   Initialise -Store option on element
                */
                //   console.log(data);
                $this.data('LinkPopup2', data);

                if (obj.dataurl != null) {
                    AjxRefresh();
                    $('' + id + '').off('click').on('click', 'tbody tr', function () {
                        //Leave
                        var init = $(this).find('td');
                        var tdid = init.attr('data-LvNewReq');
                        var tdid2 = init.attr('data-EmpLVid');
                        var tdid3 = init.attr('data-IsClose');
                        var tdid4 = init.attr('data-LvHead_Id');
                        //IT
                        var tdid5 = init.attr('data-itinvestmentid');
                        var tdid6 = init.attr('data-emppayrollid');
                        var tdid7 = init.attr('data-EmpId');

                        console.log(tdid5);
                        var forwardadtad = [tdid, tdid2, tdid3, tdid4, tdid5, tdid6, tdid7].toString();
                        var htmlurl = $(this).attr('data-htmurl');
                        var editurl = $(this).attr('data-editurl');
                        if (obj.mode.toUpperCase() == "VIEW") {
                            $('<div></div>').PartialCreateDialog({
                                height: 550,
                                width: 667,
                                form: obj.form,
                                htmlurl: htmlurl,
                                state: "open",
                                mode: obj.mode,
                                title: obj.title,
                                editurl: editurl,
                                submiturl: obj.submiturl,
                                editdata: forwardadtad,
                                forwarddata: forwardadtad,
                                returndatafunction: function (data) {
                                    obj.returndatafunction(data);
                                },
                                submitfun: function () {
                                    obj.submitfun();
                                }
                            });
                        } else if (obj.mode.toUpperCase() == "EDIT") {
                            $('<div></div>').PartialCreateDialog({
                                height: 'auto',
                                width: 'auto',
                                form: obj.form,
                                htmlurl: htmlurl,
                                state: "open",
                                mode: obj.mode, title: obj.title,
                                editurl: editurl,
                                submiturl: obj.submiturl,
                                editdata: forwardadtad,
                                forwarddata: forwardadtad,
                                submitfun: function () {
                                    obj.submitfun();
                                },
                                returndatafunction: function (data) {
                                    obj.returndatafunction(data);
                                }
                            });
                        } else if (obj.mode.toUpperCase() == "viewsubmit") {
                        }
                    });
                }
            };
            var getData = function () {
                /*
                    Get Data On Element
                */
                return $this.data('LinkPopup2');
            };
            if (fn) {
                init(obj);
            };
            function AjxRefresh() {
                id = getData().init;
                $('' + id + ' tr:gt(0)').remove();
                //  console.log(id);
                $.ajax({
                    url: getData().dataurl,
                    beforeSend: function () {
                        CardAjaxLoder(id);
                    },
                    success: function (data) {
                        if (data.status == true) {
                            $.each(data.data, function (i, k) {
                                var temp = "";
                                if (i == 0) {
                                    $.each(k.vals, function (l, j) {
                                        temp += "<th>" + j + "</th>";
                                    });
                                } else {
                                    $.each(k.vals, function (l, j) {
                                        temp += "<td>" + j + "</td>";
                                    });
                                }
                                $('' + id + ' tr:last').after("<tr class='new-lvnewreq-span2'>" + temp + "</tr>");
                            });
                        } else {
                            $('' + id + ' tr:gt(0)').remove();
                            var temp = $this.find('.show-hide-div');
                            console.log(temp);
                        }
                    },
                    complete: function () {
                        $('' + id + '').find('.CardAjaxLoaderClass').remove();
                    }
                });
            };
            return {
                /*
                *   Register Method
                */
                AjxRefresh: AjxRefresh
            };
        };
        $.fn._tooltip = function () {
            //var init = $(this);
            //init.off("mouseover").on("mouseover", function (e) {
            //    if ($('.r_tooltip_text').length == 0) {
            //        var target = $(e.target);
            //        var pagex = e.pageX;
            //        var pagey = e.pageY;
            //        console.log(pagex);
            //        // console.log(target);
            //        var off = target.offset();
            //        var hei = target.height();
            //        //   console.log(hei);
            //        var text = target.attr("data-title");
            //        var temp = "<span class=r_tooltip_text>" + text + "</span>";
            //        var htm = $(temp).css({ top: off.top + parseInt(hei) + 5 });
            //        htm.insertAfter(target);
            //    }
            //});
            //init.on("mouseout", function () {
            //    $('.r_tooltip_text').remove();
            //});
            //init.on("mouseleave", function () {
            //    $('.r_tooltip_text').remove();
            //});
        };

        $.fn.LinkPopup = function (fn) {
            var obj = $.extend({
                init: '#' + $(this).attr('id') + '',
                dataurl: null,
                htmurl: null,
                editurl: null,
                title: null,
                form: null,
                submiturl: null,
                mode: null,
                returndatafunction: null,
                submitfun: null
            }, fn);
            var id_str = $(this).attr('id');
            var id, $this = $(this);
            var parent = $(this).closest('div.dashboard-card');
            //  console.log(parent);

            //parent.css({ "width": "45%" })
            //$(this).css({ "height": "195px" });
            var init = function (data) {
                /*
                *   Initialise -Store option on element
                */
                //   console.log(data);
                $this.data('LinkPopup', data);

                if (obj.dataurl != null) {
                    AjxRefresh();
                    $('' + id + '').off('click').on('click', 'tr', function (e) {
                        //Leave
                        var init = $(this);
                        console.log(init);
                        //console
                        var tdid = init.attr('data-LvNewReq');
                        var tdid2 = init.attr('data-EmpLVid');
                        var tdid3 = init.attr('data-IsClose');
                        var tdid4 = init.attr('data-LvHead_Id');
                        //IT
                        var tdid5 = init.attr('data-itinvestmentid');
                        var tdid6 = init.attr('data-emppayrollid');
                        var tdid7 = init.attr('data-EmpId');

                        var forwardadtad = [tdid, tdid2, tdid3, tdid4, tdid5, tdid6, tdid7].toString();
                        var htmlurl = $(this).attr('data-htmurl');
                        var editurl = $(this).attr('data-editurl');
                        if (obj.mode.toUpperCase() == "VIEW") {
                            $('<div></div>').PartialCreateDialog({
                                height: 550,
                                width: 667,
                                form: obj.form,
                                htmlurl: htmlurl,
                                state: "open",
                                mode: obj.mode,
                                title: obj.title,
                                editurl: editurl,
                                submiturl: obj.submiturl,
                                editdata: forwardadtad,
                                forwarddata: forwardadtad,
                                returndatafunction: function (data) {
                                    obj.returndatafunction(data);
                                },
                                submitfun: function () {
                                    obj.submitfun();
                                }
                            });
                        } else if (obj.mode.toUpperCase() == "EDIT") {
                            $('<div></div>').PartialCreateDialog({
                                height: 560,
                                width: 1250,
                                form: obj.form,
                                htmlurl: htmlurl,
                                state: "open",
                                mode: obj.mode, title: obj.title,
                                editurl: editurl,
                                submiturl: obj.submiturl,
                                editdata: forwardadtad,
                                forwarddata: forwardadtad,
                                submitfun: function () {
                                    obj.submitfun();
                                },
                                returndatafunction: function (data) {
                                    obj.returndatafunction(data);
                                }
                            });
                        } else if (obj.mode.toUpperCase() == "viewsubmit") {
                        }
                    });
                }
            };
            var getData = function () {
                /*
                    Get Data On Element
                */
                return $this.data('LinkPopup');
            };
            if (fn) {
                init(obj);
            };
            function AjxRefresh() {
                id = getData().init;
                $('' + id + ' tr').remove();
                //  console.log(id);
                $.ajax({
                    url: getData().dataurl,
                    beforeSend: function () {
                        CardAjaxLoder(id);
                    },
                    success: function (data) {
                        if (data.status == true) {
                            $('' + id + ' tr').remove();
                            var count = 0;
                            var temp = "<thead><tr>";
                            var availableTags = [];
                            $.each(data.data, function (i, k) {
                                if (i == 0) {
                                    $.each(k, function (l, j) {
                                        if (j != null) {
                                            if (l != "RowData") {
                                                temp += "<th>" + j + "</th>";
                                            }
                                        }
                                    });
                                    temp += "</tr></thead>";
                                } else {
                                    count++;
                                    temp += '<tr id="newtd" class="new-lvnewreq-span3" data-htmurl="' + getData().htmurl + '" data-editurl="' +
                                        getData().editurl + '" data-ItinvestmentId="' + k.RowData.ItinvestmentId + '" data-emppayrollid="' +
                                        k.RowData.emppayrollid + '" data-LvNewReq="' + k.RowData.LvNewReq + '" data-EmpLVid="' + k.RowData.EmpLVid +
                                        '" data-LvHead_Id="' + k.RowData.LvHead_Id + '" data-EmpId="' + k.RowData.EmpId + '" data-IsClose="' +
                                        k.RowData.IsClose + '">';
                                    $.each(k, function (l, j) {
                                        if (l != "RowData") {
                                            temp += '<td class="">' + j + '</td>';
                                            availableTags.push(j);
                                        }
                                    });
                                    temp += "</tr>";
                                }

                            });
                            if (count > 0) {
                                $('' + id + '').append(temp);
                            }
                            $('' + id + '').find('.CardAjaxLoaderClass').remove();
                            var result = [];
                            availableTags.forEach(function (item) {
                                if (result.indexOf(item) < 0) {
                                    if (item != null) {
                                        result.push(item);
                                    }
                                }
                            });
                            $("#table-header-search-input").autocomplete({
                                source: result
                            });
                            if ($('' + id + '').prev('.table-wrapper').length == 0) {
                                $('' + id + '').parent('.card-body').prev('.card-heading')
                                    .append('<div id="expand-hide-div-id_' + id_str + '" title="Show Filter" class="btn-header expand-hide-div fa fa-fw fa-search card-header-action-btn"></div>');
                                $('' + id + '').before("<div class='table-wrapper table-tr-hide'><span class='table-header-info'><span class='table-header-search'>" +
                                 "<input type='text' class='table-header-search-input content_margine' placeholder='Search..' id='table-header-search-input_" + id_str + "'></span>" +
                                 "</span></div>");
                                $('' + id + '').after("<span class='table-header-total-row-count content_margine'></span>");
                                $('#expand-hide-div-id_' + id_str + '').off('click').on('click', function () {
                                    $('' + id + '').prev('div.table-wrapper').toggleClass('table-tr-hide');
                                });
                                $('#table-header-search-input_' + id_str + '').off('keyup').on('keyup', function (e) {
                                    if (e.which == 13) {
                                        if ($(this).val()) {
                                            var value = $(this).val().toLowerCase();
                                            $("" + id + " tr:gt(1)").filter(function (i, k) {
                                                if ($(k).text().toLowerCase().indexOf(value) > -1) {
                                                    $(k).removeClass("table-tr-hide");
                                                }
                                                else {
                                                    $(k).addClass('table-tr-hide');

                                                }
                                            });
                                        }
                                    } else if (e.keyCode == 8) {
                                        $('' + id + ' tr').removeClass("table-tr-hide");
                                    }
                                });
                            }

                        } else {
                            $('' + id + ' tr').remove();
                        }
                    },
                    complete: function () {
                        $('' + id + '').find('.CardAjaxLoaderClass').remove();
                    }
                });
            };
            return {
                /*
                *   Register Method
                */
                AjxRefresh: AjxRefresh
            };
        };
        $.fn.RenderTable = function (fn) {
            var obj = $.extend({
                init: '#' + $(this).attr('id') + '',
                dataurl: null,
                forwardData: null,
                returnfun: null,
            }, fn);

            var id, $this = $(this);
            var id_str = $(this).attr('id');
            var init = function (data) {
                $this.data('RenderTable', data);
                if (obj.dataurl != null) {
                    AjxRefresh();
                }
            };
            var getData = function () {
                /*
                    Get Data On Element
                */
                return $this.data('RenderTable');
            };
            if (fn) {
                init(obj);
            };
            function AjxRefresh() {
                id = getData().init;
                $('' + id + ' tr').remove();
                //  console.log(id);
                $.ajax({
                    url: getData().dataurl,
                    beforeSend: function () {
                        CardAjaxLoder(id);
                    },
                    data: { data: getData().forwardData },
                    success: function (data) {
                        if (data.status == true) {
                            //$('' + id + ' tr').remove();
                            var count = 0;
                            var temp = "<thead><tr>";
                            var availableTags = [];
                            $.each(data.data, function (i, k) {
                                if (i == 0) {
                                    $.each(k, function (l, j) {
                                        if (j != null) {
                                            temp += "<th>" + j + "</th>";
                                        }
                                    });
                                    temp += "</tr></thead>";
                                } else {
                                    count++;
                                    temp += "<tr class='new-lvnewreq-span3'>";
                                    $.each(k, function (l, j) {
                                        temp += "<td>" + j + "</td>";
                                        availableTags.push(j);
                                    });
                                    temp += "</tr>";
                                }
                            });

                            $('' + id + '').find('.CardAjaxLoaderClass').remove();
                            var result = [];
                            availableTags.forEach(function (item) {
                                if (result.indexOf(item) < 0) {
                                    if (item != null) {
                                        result.push(item);
                                    }
                                }
                            });
                            $("#table-header-search-input").autocomplete({
                                source: result
                            });
                            if ($('' + id + '').prev('.table-wrapper').length == 0) {
                                $('' + id + '').before("<div class='table-wrapper'><span class='table-header-info'><span class='table-header-search'>" +
                                 "<input type='text' class='table-header-search-input content_margine' placeholder='Search..' id='table-header-search-input_" + id_str + "'></span>" +
                                 "</span></div>");
                                $('' + id + '').after("<span class='table-header-total-row-count content_margine'>Total Rows : " + count + "</span>");
                                $('#table-header-search-input_' + id_str + '').off('keyup').on('keyup', function (e) {
                                    if (e.which == 13) {
                                        if ($(this).val()) {
                                            var value = $(this).val().toLowerCase();
                                            $("" + id + " tr:gt(1)").filter(function (i, k) {
                                                if ($(k).text().toLowerCase().indexOf(value) > -1) {
                                                    $(k).removeClass("table-tr-hide");
                                                }
                                                else {
                                                    $(k).addClass('table-tr-hide');

                                                }
                                            });
                                        }
                                    } else if (e.keyCode == 8) {
                                        $('' + id + ' tr').removeClass("table-tr-hide");
                                    }
                                });
                            }
                        } else {
                            $('' + id + ' tr').remove();
                            //var temp = $this.find('.show-hide-div');
                            //console.log(temp);
                        }
                        getData().returnfun();
                    },
                    complete: function () {
                    }
                });
            };
            return {
                /*
                *   Register Method
                */
                AjxRefresh: AjxRefresh
            };
        };
        $.fn.ChartHelper = function (fn) {
            var obj = $.extend({
                url: null,
                type: null,
                title: null,
            }, fn);
            /*
            Plug in use -Chart js

            Official site - https://www.chartjs.org/

            Parameters
	            url	-url to give data chart 
	            type	-chart type 
	            title	-chart title
	
            Usage
                • plugin warp around chart js 
            */
            var init = $(this);
            var id = "#" + $(this).attr('id');
            $.ajax({
                url: obj.url,
                success: function (data) {
                    var horizontalBarChartData = JSON.parse(data);
                    console.log(horizontalBarChartData);
                    var chart = $(init);
                    Chart.defaults.global.defaultFontFamily = 'Verdana';
                    var myHorizontalBar = new Chart(chart, {
                        type: obj.type,
                        data: horizontalBarChartData,
                        options: {
                            maintainAspectRatio: false,
                            elements: {
                                rectangle: {
                                    borderWidth: 0.5,
                                }
                            },
                            responsive: true,
                            legend: {
                                position: 'bottom',
                            },
                            title: {
                                //  display: true,
                                // text: obj.title
                            },
                        }
                    });
                }
            });
        };
    }));
