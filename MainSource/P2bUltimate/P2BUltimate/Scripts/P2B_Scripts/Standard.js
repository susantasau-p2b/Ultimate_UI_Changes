﻿//Developed By Chaitanya Potdar
// V 1.0.0
//05:32 PM 06/01/2017

(function (factory) {
    if (typeof define === "function" && define.amd) {
        define(["jquery"], factory);
    } else {
        factory(jQuery);
    }
}
    (function ($) {
        "use strict";
        function createObjectDataFromFormId(formId) {
            const formData = $(formId);
            const allData = new FormData(formData[0]);
            const allFormData = formDataToObject(allData);
            //formData.find('table').each(function () {
            //    let name = this.id;
            //    let value = this.value || null;
            //    allData.append(name, value);
            //});
            return allFormData;
        }
        function compareDataAndSetFlag(formIdForCompareData, orgDataForCompare, DtDataForCompare) {
            let APIDataForCompare;
            const formDataForCompare = createObjectDataFromFormId(formIdForCompareData);
            if (formDataForCompare) {
                if (orgDataForCompare && orgDataForCompare.Id) {
                    APIDataForCompare = orgDataForCompare;
                } else {
                    APIDataForCompare = DtDataForCompare;
                }
                if (APIDataForCompare && APIDataForCompare.Id) {
                    formDataForCompare.forEach((value, key) => {
                        if (APIDataForCompare[key] !== value) {
                            return 'E';
                        }
                    });
                }
                
            }
            return 'O';
        }
        function formDataToObject(formData) {
            const obj = {};
            if (formData instanceof FormData) {
                formData.forEach((value, key) => {
                    if (obj[key]) {
                        if (Array.isArray(obj[key]) && value !== "null") {
                            obj[key].push(value);
                        } else if (value !== "null") {
                            obj[key] = [obj[key], value];
                        }
                    } else if (value !== "null") {
                        obj[key] = value;
                    } else {
                        obj[key] = null;
                    }
                });
                return obj;
            } else {
                return formData;
            }
        }
        function P2bObjectFormattedData(classObj) {
            const serializableArray = classObj.getData();
            const mergedObject = serializableArray.reduce((acc, obj) => {
                return { ...acc, ...obj };
            }, {});
            return mergedObject;
        }

        function P2bBindAllDataForCreate(createFormId, allTableIds) {
            var arrayOfObjects = {};
            const formDataHandlingClass = new P2bFormDataHandlingClass();
            const onlyFormDataForCreate = createObjectDataFromFormId(createFormId);
            var tableData = formDataHandlingClass.P2bCreateListOfTableDataWithAction(onlyFormDataForCreate, allTableIds);
            var formEntityData = formDataHandlingClass.P2bCreateListOfTableData(onlyFormDataForCreate, allTableIds);
            formDataHandlingClass.addData(formEntityData);
            var getAllObjectData = P2bObjectFormattedData(formDataHandlingClass);
            arrayOfObjects["UserClass"] = { GroupId: singleData['GroupId'], Action: 'C', UserId: singleData['EmpCode'], AuthorizeStatus, AutoAuthorization: singleData['AutoAuthorization'] };
            arrayOfObjects["EntityClass"] = { FormData: getAllObjectData, TableData: tableData };
            return arrayOfObjects;
        }
        function P2bBindAllDataForEdit(id) {
            var arrayOfObjects = {};
            const formDataHandlingClass = new P2bFormDataHandlingClass();
            formDataHandlingClass.addData({Id:id});
            var getAllObjectData = P2bObjectFormattedData(formDataHandlingClass);
            arrayOfObjects["UserClass"] = { GroupId: singleData['GroupId'], Action: 'E', UserId: singleData['EmpCode'], AuthorizeStatus, AutoAuthorization: singleData['AutoAuthorization'] };
            arrayOfObjects["EntityClass"] = getAllObjectData;
            return arrayOfObjects;
        }
        function P2bBindAllDataForEditSave(EditFormId, idForEdit, allTableIds, originalObjectData, DTObjectData) {
            var arrayOfObjects = {};
            const formDataHandlingClass = new P2bFormDataHandlingClass();
            const onlyFormDataForEditSave = createObjectDataFromFormId(EditFormId);
            var tableData = formDataHandlingClass.P2bCreateListOfTableDataWithAction(onlyFormDataForEditSave, allTableIds);
            var formEntityData = formDataHandlingClass.P2bCreateListOfTableData(onlyFormDataForEditSave, allTableIds);
            formDataHandlingClass.addData(formEntityData);
            var getAllObjectData = P2bObjectFormattedData(formDataHandlingClass);
            arrayOfObjects["UserClass"] = { GroupId: singleData['GroupId'], Action: 'E', UserId: singleData['EmpCode'], AuthorizeStatus, AutoAuthorization: singleData['AutoAuthorization'] };
            arrayOfObjects["EntityClass"] = { Id: idForEdit, FormData: getAllObjectData, TableData: tableData };
            //arrayOfObjects["EntityClass"] = { OriginalData: originalObjectData, DtData: DTObjectData, idForEdit, FormData: getAllObjectData };
            return arrayOfObjects;
        }
        function P2bBindAllDataForDelete(id) {
            var arrayOfObjects = {};
            const formDataHandlingClass = new P2bFormDataHandlingClass();
            formDataHandlingClass.addData({ Id: id });
            var getAllObjectData = P2bObjectFormattedData(formDataHandlingClass);
            arrayOfObjects["UserClass"] = { GroupId: singleData['GroupId'], Action: 'D', UserId: singleData['EmpCode'], AuthorizeStatus, AutoAuthorization: singleData['AutoAuthorization'] };
            arrayOfObjects["EntityClass"] = getAllObjectData;
            return arrayOfObjects;
        }

        function P2bBindAllDataForApproveReject(approveRejectFormId, idForEdit, IsAuthorized, DTObjectData) {
            var arrayOfObjects = {};
            const allDataForApproveReject = new P2bFormDataHandlingClass();
            const onlyFormDataForApproveReject = createObjectDataFromFormId(approveRejectFormId);
            allData = formDataToObject(allData);
            allDataForApproveReject.addData(DTObjectData, allData, { IsAuthorized: IsAuthorized });
            var getAllObjectData = P2bObjectFormattedData(allDataForApproveReject);
            arrayOfObjects["UserClass"] = { GroupId: singleData['GroupId'], Action: 'E', UserId: singleData['EmpCode'], AuthorizeStatus, AutoAuthorization: singleData['AutoAuthorization'] };
            arrayOfObjects["EntityClass"] = { DtData: getAllObjectData, Id: idForEdit, FormData: allData, IsAuthorized };
            return arrayOfObjects;
        }

        function P2bBindAllDataForLookUp(Ids) {
            var arrayOfObjects = {};
            arrayOfObjects["UserClass"] = { GroupId: singleData['GroupId'], Action: 'A', UserId: singleData['EmpCode'], AuthorizeStatus, AutoAuthorization: singleData['AutoAuthorization'] };
            arrayOfObjects["EntityClass"] = { SkipIds: Ids };
            return arrayOfObjects;
        }
        function ApproveRejectBtnClickFunc(formId, init, maindialog, titleText, approveRejectValue, rowId, approveRejectUrl, gridreloadname, allModifiedData) {
            var x = PerformValidations(formId);
            var y = true;
            //if (fn != undefined) {
            //    if (fn.validurl != null && x == true) {
            //        var allDataForAPI = $.param(P2bBindAllDataForCreate(forwardserializedata, nameofthelookuptable));
            //        var chkajx = $.ajax({
            //            url: fn.validurl,
            //            method: "POST",
            //            async: false,
            //            data: allDataForAPI,
            //            beforeSend: function () {
            //                $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('disable').addClass('submitbtndisable');
            //                ajaxloaderv2('body');
            //            },
            //        });
            //        chkajx.done(function (msg) {
            //            $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('enable').removeClass('submitbtndisable');
            //            // $('.ajax_loder').parents('div').remove();
            //            ajaxLoderRemove();
            //            if (msg.success == true) {
            //                //success event
            //                y = msg.success;
            //            } else {
            //                y = msg.success;
            //                var newDiv = $(document.createElement('div'));
            //                var htmltag = "";
            //                for (var i = 0; i < msg.responseText.length; i++) {
            //                    htmltag += '<span style="float:left;display:block;margin:2px;"><span class="ui-icon ui-icon-alert" style="float:left;display:block"></span><span style="width:80%;"> ' + msg.responseText[i] + '</span></span>';
            //                }
            //                newDiv.html(htmltag);
            //                newDiv.dialog({
            //                    autoOpen: false,
            //                    title: "Validation",
            //                    height: 250, width: 400, modal: true,
            //                    buttons: {
            //                        Ok: function (e) {
            //                            newDiv.dialog("close");
            //                            newDiv.remove();
            //                        }
            //                    }
            //                });
            //                newDiv.dialog('open');
            //                $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
            //            }

            //        });
            //        chkajx.fail(function (xhr, status, error) {
            //            $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('enable').removeClass('submitbtndisable');
            //            // $('.ajax_loder').parents('div').remove();
            //            ajaxLoderRemove();
            //            y = false;
            //            var newDiv = $(document.createElement('div'));
            //            var htmltag = '<p><span class="ui-icon ui-icon-alert" style="float:left;margin-right:10px"></span> ' + xhr.status + '"-"' + xhr.statusText + '';
            //            htmltag += '</p>';
            //            newDiv.html(htmltag);
            //            newDiv.dialog({
            //                autoOpen: false,
            //                title: "Information",
            //                height: 130,
            //                width: 250,
            //                modal: true,
            //                buttons: {
            //                    Ok: function () {
            //                        newDiv.remove();
            //                        newDiv.dialog("close");
            //                        $(init).dialog("close");
            //                    }
            //                }
            //            });
            //            newDiv.dialog('open');
            //            $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
            //        });
            //    }
            //}
            if (x == false || y == false) {
                return false;
            }
            var newDiv2 = $(document.createElement('div'));
            var htmltag = `<p><span class="ui-icon ui-icon-alert" style="float:left;margin-right:10px"></span> Are You Sure Want To ${titleText} Record???`;
            htmltag += '</p>';
            htmltag += `<form id='ApproveRejectCommentForm'>
                <label for='AuthorizedComment'>Comment:</label>
                <textarea id='AuthorizedComment' class='must' name='AuthorizedComment'></textarea>
            </form>`;
            newDiv2.html(htmltag);            
            newDiv2.dialog({
                autoOpen: false,
                title: `${titleText} Confirmation !`,
                height: 170,
                width: 305,
                closeOnEscape: false,
                beforeClose: function (e) {
                    $(newDiv2).remove(); RemoveErrTag();
                },
                open: function () {
                    //$('.ui-dialog-buttonpane').find('button:contains("Confirm")').button().button('disable').addClass('submitbtndisable');
                    
                },
                modal: true,
                buttons: {
                    Confirm: function () {
                        var commentFieldValidation = PerformValidations('#ApproveRejectCommentForm');
                        if (!commentFieldValidation) {
                            return false;
                        }
                        var allDataForAPI = JSON.stringify(P2bBindAllDataForApproveReject('#ApproveRejectCommentForm', rowId, approveRejectValue, allModifiedData));
                        var approveRejectAjaxData = $.ajax({
                            url: approveRejectUrl,
                            method: 'POST',
                            contentType: 'application/json',
                            data: allDataForAPI,
                            beforeSend: function () {
                                $('.ui-dialog-buttonpane').find('button:contains("Confirm")').button().button('disable').addClass('submitbtndisable');
                                ajaxloaderv2('body');

                            },
                        });
                        approveRejectAjaxData.done(function (msg) {
                            var htmltag = "";
                            $('.ui-dialog-buttonpane').find('button:contains("Confirm")').button().button('enable').removeClass('submitbtndisable');
                            ajaxLoderRemove();
                            if (msg.MessageCode == 200) {
                                var newDiv = $(document.createElement('div'));
                                htmltag += '<span class="ajax-action-class-container"><span style="float:left;display:block"><i class="fa fa-fw fa-3x fa-check-circle-o ajax-success-icon" aria-hidden="true"></i></span><span class="ajax-action-text"> ' + msg.Message + '</span></span>';

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
                                            //if (fn != undefined) {
                                            //    if (fn.datatablename != null) {
                                            //        var table = $(fn.datatablename).DataTable();
                                            //        table.ajax.reload();
                                            //    }
                                            //}
                                            //NewIds = [];
                                            //olddata = [];
                                            //OldIds = [];
                                            RemoveLookupTableElement(formId);
                                            jQuery(init).find('select').empty().append("<option value=0 selected=true>-Select-</option>").selectmenu().selectmenu().selectmenu("refresh");
                                            jQuery(init).find('input').empty();
                                            jQuery(init).find('textarea').empty();
                                            //jQuery(init).find(nameidclassofbuttontodisable).button().button('enable').removeClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                                            //jQuery(init).find(nameidclassofbuttontodisable).button().button('enable').removeClass('ButtonHover');
                                            //jQuery(init).find(nameofthelookuptable).find('tr td').parent().remove();
                                            newDiv.dialog("close");
                                            jQuery(maindialog).dialog("close");
                                            jQuery(newDiv2).dialog("close");

                                        }
                                    }
                                });
                                newDiv.dialog('open');
                            } else {
                                var newDiv = $(document.createElement('div'));
                                htmltag += '<span class="ajax-action-class-container"><span style="float:left;display:block"><i class="fa fa-fw fa-3x fa-exclamation-circle ajax-error-icon" aria-hidden="true"></i></span><span class="ajax-action-text"> ' + msg.Message + '</span></span>';

                                newDiv.html(htmltag);
                                newDiv.dialog({
                                    autoOpen: false,
                                    title: "Error",
                                    height: 250, width: 400, modal: true,
                                    buttons: {
                                        Ok: function (e) {
                                            newDiv.dialog("close");
                                            newDiv.remove();
                                            newDiv2.dialog('close');
                                            $(newDiv).remove();

                                        }
                                    }
                                });
                                newDiv.dialog('open');
                            }
                            $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                        });
                        approveRejectAjaxData.fail(function (jqXHR, textStatus) {
                            $('.ui-dialog-buttonpane').find('button:contains("Confirm")').button().button('enable').removeClass('submitbtndisable');
                            ajaxLoderRemove();
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
            $('.ui-dialog-buttonpane').find('button:contains("Confirm")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
            $('.ui-dialog-buttonpane').find('button:contains("Cancel")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-closethick"></span>');

        }
        function P2bGetAllFilteredNames(objForFilterData) {
            var arrayOfUncommonKeys = [];
            if (objForFilterData && objForFilterData['FieldNames'].length > 0 && objForFilterData.OriginalData && objForFilterData.ModifiedData) {
                $.each(objForFilterData['FieldNames'],(itemValue,itemKey) => {
                    if (objForFilterData.OriginalData[itemKey] !== objForFilterData.ModifiedData[itemKey]) {
                        arrayOfUncommonKeys.push(itemKey);
                    }
                });
                
            }
            return arrayOfUncommonKeys;
        }
        function P2bCreateAuthorizationDialog(obj) {
            var tableWithAllNewData = '';
            var tableWithAllOldData = '';
            var authDialogDiv = $("#Autho-Dialog");
            if (authDialogDiv.length === 0) {
                authDialogDiv = $(document.createElement('div')).attr('id', 'Autho-Dialog');
            }
            var tableDataDiv = $("#Auth-Table-Data");
            if (tableDataDiv.length === 0) {
                tableDataDiv = $(document.createElement('div')).attr('id', 'Auth-Table-Data');
            }
            var commentDataDiv = $("#Comment-Data");
            if (commentDataDiv.length === 0) {
                commentDataDiv = $(document.createElement('div')).attr('id', 'Comment-Data');
            }

            var newDataTable = $('#New-Data-Table');
            if (newDataTable.length === 0) {
                newDataTable = $(document.createElement('table')).attr({ 'id':'New-Data-Table','class': 'fiter-data-table'});
                $(newDataTable).append('<caption>Compared Data</caption>');
                $(newDataTable).append('<tr><th>Label</th><th>Old Data</th><th>New Data</th></tr>');
            }

            var commentField = `<form id='FormApproveReaject'><label>Comment</label><textarea id='Comment' name='Comment'></textarea></form>`

            var labelAndFieldNameObject = {};
            if (obj && obj.FormName) {
                $(`${obj.FormName} label`).each(function () {
                    var fieldId = $(this).attr('for');
                    var labelText = $(this).text().trim();
                    var field = $('#' + fieldId);
                    if (field.length > 0) {
                        if (labelText === 'Yes' || labelText === 'No') {
                            const allKeys = Object.keys(labelAndFieldNameObject);
                            const lastItem = allKeys[allKeys.length - 1];
                            labelAndFieldNameObject[lastItem] = field.attr('name');
                        } else if (fieldId.includes('_DDL')) {
                            var onlyId = fieldId.replace('-button', '');
                            labelAndFieldNameObject[labelText] = $('#' + onlyId).attr('name');
                        } else {
                            labelAndFieldNameObject[labelText] = field.attr('name');
                        }                        
                    }else {
                        labelAndFieldNameObject[labelText] = null;
                    }
                });
            }
            var arrayOfFieldNames = labelAndFieldNameObject ? Object.values(labelAndFieldNameObject) : [];
            var uncommonKeys = P2bGetAllFilteredNames({ FieldNames: arrayOfFieldNames, OriginalData: obj['OriginalData'], ModifiedData: obj['ModifiedData'] })

            function addDataIntoTable(labelAndFieldName, OrgData, ModData) {
                tableWithAllNewData = '';
                $.each(labelAndFieldName, (label, name) => {
                    if (OrgData[name] !== ModData[name]) {
                        tableWithAllNewData += `<tr><td class='td-label'>${label}</td><td class='td-org-data td-data'>${OrgData[name]}</td><td class='td-mod-data td-data'>${ModData[name]}</td></tr>`;
                    } else {
                        tableWithAllNewData += `<tr><td class='td-label'>${label}</td><td class='td-org-data'>-</td><td class='td-mod-data'>${ModData[name]}</td></tr>`;
                    }
                });
            }

            if (uncommonKeys.length > 0) {
                let filteredLabelAndName = {};
                $.each(labelAndFieldNameObject, function (key, value) {
                    if (uncommonKeys.includes(value)) {
                        filteredLabelAndName[key] = value;
                    }
                });

                addDataIntoTable( filteredLabelAndName, obj.OriginalData, obj.ModifiedData);
            } else {
                addDataIntoTable( labelAndFieldNameObject, obj.OriginalData, obj.ModifiedData);
            }

            $(newDataTable).append(tableWithAllNewData);
            $(authDialogDiv).append(tableDataDiv).append(commentDataDiv);
            $(tableDataDiv).append(newDataTable);

            $(authDialogDiv).dialog({
                width: 500,
                height: 400,
                modal: false,
                closeOnEscape: false,
                position: { my: "right center", at: "right center", of: window },
                title: 'Company Authorization',
                open: function () {
                    $(this).find("select").selectmenu('disable').addClass("ui-selectmenu-text");
                    $(this).find('input').prop('disabled', true).css("background-color", "rgba(241, 241, 241, 0.66)");
                    $(this).find('textarea').prop('disabled', true).css("background-color", "rgba(241, 241, 241, 0.66)");
                },

                //buttons: {
                //    Approve: function () {
                //        approveRejectAjaxCall();
                //    },
                //    Reject: function () {
                //        alert("Hi this is Reject button");
                //    },
                //    Cancel: function () {
                //        $(this).dialog('close');
                //    }
                //}                
            });
            //$('.ui-dialog-buttonpane .ui-dialog-buttonset .ui-button:contains("Reject")').hover(
            //    function () {
            //        $(this).css("background-color", "rgba(241, 0, 0, 0.66)");
            //},
            //    function () {
            //        $(this).css("background-color", "");
            //    })
        }

        function P2bCreateHelpDialog(obj) {
            var helpDialogDiv = $("#HelpDialog");
            if (helpDialogDiv.length === 0) {
                helpDialogDiv = $(document.createElement('div')).attr('id', 'HelpDialog');
            }
            var helpDocument;

            $(helpDialogDiv).dialog({
                autoOpen: false,
                height: 500,
                width: 700,
                modal: true,
                closeOnEscape: false,
                title: "Help",
                beforeClose: function () {

                },
                open: function (event, ui) {
                    var formName = '';
                    if (obj && typeof obj === 'object') {
                        if (obj.FormName) {
                            formName = obj.FormName.replace('#', '');
                        }
                    }
                    var helpAjaxData = $.ajax({
                        url: "http://localhost/P2BUltimate2.0Api" + '/GetHelp',
                        method: 'POST',
                        data: { FormName: formName },
                        //success:
                    });
                    helpAjaxData.done(function (value) {
                        helpDocument = value.Data;
                        var sectionHtml = `<div id="accordion">`;
                        if (helpDocument) {
                            $.each(helpDocument, function (i, j) {
                                $.each(j, function (key, value) {
                                    sectionHtml += `<h3> ${key} </h3> 
                                <div>
                                    <p>${value}</p>
                                </div>`
                                })
                            })
                            sectionHtml += '</div>'
                        }
                        $(helpDialogDiv).html(sectionHtml);
                        $("#accordion").accordion({
                            collapsible: true
                        });
                    });
                    helpAjaxData.fail(function (value) {
                        alert("Something went wrong.");
                    });
                    $('#accordion').accordion("refresh");
                },
                buttons: {
                    Close: function () {
                        $(this).dialog("close");
                        //$(this).dialog('destroy');
                        $('#accordion').accordion("refresh");
                    }
                }
            })
            $(helpDialogDiv).dialog('open');
        }

        $.fn.P2BDatePicker = function () {
            /*
           Dependancy datetimepickerjs
           */
            this.datetimepicker({
                lang: 'en',
                timepicker: false,
                format: 'd/m/Y',
                formatDate: 'dd/MM/yyyy',
                yearEnd: 2080
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
            /*
         Dependancy datetimepickerjs
         */
            this.datetimepicker({
                lang: 'en',
                inline: true,
                timepicker: false,
                format: 'd/m/Y',
                formatDate: 'dd/MM/yyyy'
            });
        };
        $.fn.P2BTimeInlinePicker = function () {
            /*
         Dependancy datetimepickerjs
         */
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
        $.fn.P2BGrid = function (ColNames, ColModel, SortName, Caption, url, width, height, pager, extraparam) {

            /*
      
            Dependancy JqGridJs
    
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
                mtype: "POST",
                colNames: ColNamesData,
                colModel: ColModelData,
                rowNum: extraparam.rowNum,
                rowList: [10, 20, 30, 100],
                pager: pager,
                sortname: SortName,
                multiselect: extraparam.multiple,
                multiboxonly: extraparam.multiple,
                viewrecords: true,
                sortorder: "asc",
                caption: Caption,
                width: width,
                height: height,
                //beforeSelectRow: function (rowId, e) {
                //    return $(e.target).is("input:checkbox");
                //}
            });

            this.jqGrid('navGrid', pager, { edit: false, add: false, del: false });
            this.jqGrid('setGridParam', {
                url: url,
                page: 1,
                search: false,
                postData: { filters: '', _search: false, searchField: null, searchOper: null, searchString: null }
            }, true).trigger('reloadGrid');
            this.trigger('reyGrid', [{ _search: false, searchField: null, searchOper: '', searchString: '' }]);

            if (extraparam.selectall == true && extraparam.multiple == true) {
                var init = $(this);
                if ($(`${pager} :contains("SelectAll")`).length === 0) {
                    this.jqGrid('navGrid', pager, { edit: false, add: false, del: false, search: false }).navButtonAdd(pager, {
                        caption: "SelectAll",
                        buttonicon: "ui-icon-arrow-2-n-s",
                        onClickButton: function () {
                            ajaxloader(init);
                            $(init).find("div .ajax_loder").show();
                            if (JqGridCheck.select_all == false) {
                                $.ajax({
                                    url: url,
                                    method: "POST",
                                    success: function (data) {
                                        var a = [];
                                        var EmpCode = [];
                                        // var empcodeindex = ColModel.indexOf("EmpCode");
                                        var empcodeindex = ColModel.indexOf("Id");
                                        // var empcodeindex = 8;
                                        $.each(data.rows, function (i, k) {
                                            a.push(k[empcodeindex]);
                                            EmpCode.push(k[empcodeindex]);
                                        });
                                        if (a.length > 0) {
                                            alert("Total Selected Record : " + parseInt(a.length) + "");
                                            $(init).find("div .ajax_loder").hide();
                                            $('#cb_' + $(init).attr('id') + '').trigger('click');
                                            JqGridCheck.select_all = true;
                                            $('#emp_Id').val(EmpCode);

                                            localStorage.setItem("LEAVECREDITTRAILBALANCE", a);
                                            JqGridCheck.Set(a);
                                        }
                                    },
                                    data: { rows: 0, page: 0, sord: "asc", filter: $('#TextPayMonth').val() }
                                });
                            } else {
                                $('#cb_' + $(init).attr('id') + '').trigger('click');
                                JqGridCheck.Del();
                                $(init).find("div .ajax_loder").hide();
                            }

                        },
                        position: "last"
                    });
                }
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

        $.fn.P2BGrid1 = function (ColNames, ColModel, SortName, Caption, url, width, height, pager, extraparam) {
            /*
      
            Dependancy JqGridJs
    
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
                rowList: [10, 20, 30, 100],
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
            this.trigger('reyGrid', [{ _search: false, searchField: null, searchOper: '', searchString: '' }]);
            this.jqGrid('filterToolbar', { stringResult: true, searchOnEnter: false, defaultSearch: 'cn' });
            if (extraparam.selectall == true && extraparam.multiple == true) {
                var init = $(this);
                if ($(`${pager} :contains("SelectAll")`).length === 0) {
                    this.jqGrid('navGrid', pager, { edit: false, add: false, del: false, search: false }).navButtonAdd(pager, {
                        caption: "SelectAll",
                        buttonicon: "ui-icon-arrow-2-n-s",
                        onClickButton: function () {
                            ajaxloader(init);
                            $(init).find("div .ajax_loder").show();
                            if (JqGridCheck.select_all == false) {
                                $.ajax({
                                    url: url,
                                    method: "POST",
                                    success: function (data) {
                                        var a = [];
                                        var EmpCode = [];
                                        // var empcodeindex = ColModel.indexOf("EmpCode");
                                        var empcodeindex = ColModel.indexOf("Id");
                                        // var empcodeindex = 8;
                                        $.each(data.rows, function (i, k) {
                                            a.push(k[empcodeindex]);
                                            EmpCode.push(k[empcodeindex]);
                                        });
                                        if (a.length > 0) {
                                            alert("Total Selected Record : " + parseInt(a.length) + "");
                                            $(init).find("div .ajax_loder").hide();
                                            $('#cb_' + $(init).attr('id') + '').trigger('click');
                                            JqGridCheck.select_all = true;
                                            $('#emp_Id').val(EmpCode);

                                            localStorage.setItem("LEAVECREDITTRAILBALANCE", a);
                                            JqGridCheck.Set(a);
                                        }
                                    },
                                    data: { rows: 0, page: 0, sord: "asc", filter: $('#TextPayMonth').val() }
                                });
                            } else {
                                $('#cb_' + $(init).attr('id') + '').trigger('click');
                                JqGridCheck.Del();
                                $(init).find("div .ajax_loder").hide();
                            }

                        },
                        position: "last"
                    });
                }
            }
            //if (extraparam.selectall == true && extraparam.multiple == true) {
            //    $("#pager22_left").find("td").remove();
            //    var init = $(this);
            //    if ($(`${pager} :contains("SelectAll")`).length === 0) {
            //        this.jqGrid('navGrid', pager, { edit: false, add: false, del: false, search: false }).navButtonAdd(pager, {
            //            caption: "SelectAll",
            //            buttonicon: "ui-icon-arrow-2-n-s",
            //            onClickButton: function () {
            //                if (JqGridCheck.select_all == false) {
            //                    $.ajax({
            //                        url: url,
            //                        method: "POST",
            //                        success: function (data) {
            //                            var a = [];
            //                            var EmpCode = [];
            //                            //var empcodeindex = ColModel.indexOf("EmpCode");
            //                            var empcodeindex = ColModel.indexOf("Id");
            //                            $.each(data.rows, function (i, k) {
            //                                a.push(k[empcodeindex]);
            //                                EmpCode.push(k[empcodeindex]);
            //                            });
            //                            if (a.length > 0) {
            //                                alert("Total Selected Record : " + parseInt(a.length) + "");
            //                                $('#cb_' + $(init).attr('id') + '').trigger('click');
            //                                JqGridCheck.select_all = true;
            //                                $('#emp_Id').val(EmpCode);

            //                                localStorage.setItem("LEAVECREDITTRAILBALANCE", a);
            //                                JqGridCheck.Set(a);
            //                            }
            //                        },
            //                        data: { rows: 0, page: 0, sord: "asc", filter: $('#TextPayMonth').val() }
            //                    });
            //                } else {
            //                    $('#cb_' + $(init).attr('id') + '').trigger('click');
            //                    JqGridCheck.Del();
            //                }
            //            },
            //            position: "last"
            //        });
            //    }
            //}
        };

        $.extend($.fn.P2BGrid1, {
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
        $.extend($.fn.P2BGrid1, {
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
        var ajaxLoderRemove = function (init) {
            if (!init) {
                $('.ajax_loder').parents('div').remove();
            } else {
                $(init).find("div .ajax_loder").remove();
            }
            $(window).off('beforeunload');
        };
        var RemoveErrTag = function () {
            $('.error').remove();
        };
        function RemoveLookupTableElement(form) {
            var formname = $(form);
            formname.find('table.lookuptableselected').find('tr td').parent().remove();
        }
        $.CheckSessionExitance = function () {
            $.ajax({
                url: "./Login/CheckSessionExitance",
                async: false,
                success: function (data) {
                    if (data.success == false) {
                        window.location.reload();
                    }
                }
            });
        };
        $.fn.P2BCreateDialog = function (creaturl, creadata, url, forwarddata, maindialogtitle, state, submiturl, submitnameformforserilize, savemessage, errormessage, gridreloadname, height, width, nameofthelookuptable, nameidclassofbuttontodisable, returnfunctiondata, fn) {
            jQuery(this).trigger('reset');
            //$('select').removeAttr('style');
            //var createSubmitUrl = absoluteUrlPath + submiturl;
            var init = jQuery(this);
            var ajaxdata, createajaxdata;
            nameidclassofbuttontodisable = '.popup-content-icon-edit,.popup-content-icon-remove,.popup-content-icon-view';
            var maindailog = jQuery(init).dialog({
                iconButtons: [{
                    text: "Help",
                    icon: "ui-icon-help"
                }],
                autoOpen: false,
                height: height,
                width: width,
                modal: true,
                closeOnEscape: false,
                title: maindialogtitle,
                beforeClose: function () {
                    RemoveLookupTableElement(submitnameformforserilize);
                    jQuery(init).find('select').empty().append("<option value=0 selected=true>-Select-</option>").selectmenu().selectmenu("refresh");
                    jQuery(init).find('input').empty();
                    jQuery(init).find('textarea').empty();
                    //jQuery(init).find(nameidclassofbuttontodisable).button().button('enable').removeClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                    jQuery(init).find(nameidclassofbuttontodisable).button().button('enable').removeClass('ButtonHover');
                    jQuery(init).find(nameofthelookuptable).find('tr td').parent().remove(); RemoveErrTag();
                },
                open: function (event, ui) {
                    $.CheckSessionExitance();
                    $('.ui-dialog-titlebar-help').html('<span class="ui-button-icon ui-icon ui-icon-help"></span>');
                    $('.ui-dialog-titlebar-help').click(function () {
                        P2bCreateHelpDialog({ FormName: submitnameformforserilize });
                        //helpfun("create", "" + submitnameformforserilize.slice("4") + "");
                    });
                    //jQuery(init).find(nameidclassofbuttontodisable).button().button('enable').removeClass('ButtonHover');
                    createajaxdata = $.ajax({
                        url: creaturl,
                        method: 'POST',
                        contentType: 'application/json',
                        data: { data: creadata }
                    });
                    createajaxdata.done(function (value) {
                        returnfunctiondata(value);
                    });
                    //jQuery(init).find(nameidclassofbuttontodisable).button().button('disable').addClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                    jQuery(init).find(nameidclassofbuttontodisable).button().button('disable').addClass('ButtonHover');
                    OnpageAlter();
                    MakeRadioBtnChecked();
                    $.removeDisble(submitnameformforserilize);
                },
                buttons: {
                    Submit: function () {
                        var x = PerformValidations(submitnameformforserilize);
                        var y = true;
                        if (fn != undefined) {
                            if (fn.validurl != null && x == true) {
                                var allDataForAPI = JSON.stringify(P2bBindAllDataForCreate(submitnameformforserilize));
                                //var allDataForAPI = $.param(P2bBindAllDataForCreate(submitnameformforserilize));
                                var chkajx = $.ajax({
                                    url: fn.validurl,
                                    method: "POST",
                                    contentType: 'application/json',
                                    async: false,
                                    data: allDataForAPI,
                                    beforeSend: function () {
                                        $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('disable').addClass('submitbtndisable');
                                        ajaxloaderv2('body');
                                    },
                                });
                                chkajx.done(function (msg) {
                                    $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('enable').removeClass('submitbtndisable');
                                    //$('.ajax_loder').parents('div').remove();
                                    ajaxLoderRemove();
                                    if (msg.MessageCode == 200) {
                                        //success event
                                        y = true;
                                    } else {
                                        y = false;
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
                                                    if (fn != undefined) {
                                                        if (fn.CloseRetunFun != null) {
                                                            fn.CloseRetunFun();
                                                        }
                                                    }
                                                }
                                            }
                                        });
                                        newDiv.dialog('open');
                                        $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                                    }

                                });
                                chkajx.fail(function (xhr, status, error) {
                                    $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('enable').removeClass('submitbtndisable');
                                    //$('.ajax_loder').parents('div').remove();
                                    ajaxLoderRemove();
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
                        var allDataForAPI = JSON.stringify(P2bBindAllDataForCreate(submitnameformforserilize, nameofthelookuptable));
                        ajaxdata = $.ajax({
                            url: submiturl,
                            method: "POST",
                            contentType: 'application/json',
                            data: allDataForAPI,
                            beforeSend: function () {
                                $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('disable').addClass('submitbtndisable');
                                ajaxloaderv2('body');
                            },
                        });
                        ajaxdata.done(function (msg) {
                            $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('enable').removeClass('submitbtndisable');
                            // $('.ajax_loder').parents('div').remove();
                            ajaxLoderRemove();
                            var htmltag = "";
                            if (msg.MessageCode == 200) {
                                var newDiv = $(document.createElement('div'));
                                //for (var i = 0; i < msg.Message.length; i++) {
                                htmltag = '<span class="ajax-action-class-container"><span style="float:left;display:block"><i class="fa fa-fw fa-3x fa-check-circle-o ajax-success-icon" aria-hidden="true"></i></span><span class="ajax-action-text"> ' + msg.Message + '</span></span>';
                                //}
                                newDiv.html(htmltag);
                                newDiv.dialog({
                                    autoOpen: false,
                                    title: "Information",
                                    height: 400, width: 250, modal: true,
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
                                            jQuery(init).find('select').empty().append("<option value=0 selected=true>-Select-</option>").selectmenu().selectmenu("refresh");
                                            jQuery(init).find('input').empty();
                                            jQuery(init).find('textarea').empty();
                                            //jQuery(init).find(nameidclassofbuttontodisable).button().button('enable').removeClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                                            jQuery(init).find(nameidclassofbuttontodisable).button().button('enable').removeClass('ButtonHover');
                                            jQuery(init).find(nameofthelookuptable).find('tr td').parent().remove();
                                            newDiv.dialog("close");
                                            jQuery(maindailog).dialog("close");
                                            if (fn != undefined) {
                                                if (fn.CloseRetunFun != null) {
                                                    fn.CloseRetunFun();
                                                }
                                            }
                                        }
                                    }
                                });
                                newDiv.dialog('open');
                                $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                            } else {
                                var newDiv = $(document.createElement('div'));
                                for (var i = 0; i < msg.responseText.length; i++) {
                                    htmltag += '<span class="ajax-action-class-container"><span style="float:left;display:block"><i class="fa fa-fw fa-3x fa-exclamation-circle ajax-error-icon" aria-hidden="true"></i></span><span class="ajax-action-text"> ' + msg.responseText[i] + '</span></span>';
                                }
                                newDiv.html(htmltag);
                                newDiv.dialog({
                                    autoOpen: false,
                                    title: "Error",
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
                        ajaxdata.fail(function (jqXHR, textStatus) {
                            $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('enable').removeClass('submitbtndisable');
                            //$('.ajax_loder').parents('div').remove();
                            ajaxLoderRemove();
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

        $.fn.P2BCreateDialog1 = function (creaturl, creadata, url, forwarddata, maindialogtitle, state, submiturl, submitnameformforserilize, savemessage, errormessage, gridreloadname, height, width, nameofthelookuptable, nameidclassofbuttontodisable, returnfunctiondata, fn) {
            jQuery(this).trigger('reset');
            var init = jQuery(this);
            var ajaxdata, createajaxdata;
            nameidclassofbuttontodisable = '.popup-content-icon-edit,.popup-content-icon-remove,.popup-content-icon-view';
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
                    jQuery(init).find('select').empty().append("<option value=0 selected=true>-Select-</option>").selectmenu().selectmenu("refresh");
                    jQuery(init).find('input').empty();
                    jQuery(init).find('textarea').empty();
                    //jQuery(init).find(nameidclassofbuttontodisable).button().button('enable').removeClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                    jQuery(init).find(nameidclassofbuttontodisable).button().button('enable').removeClass('ButtonHover');
                    jQuery(init).find(nameofthelookuptable).find('tr td').parent().remove(); RemoveErrTag();
                },
                open: function (event, ui) {
                    $.CheckSessionExitance();
                    $('.ui-dialog-titlebar-help').html('<span class="ui-button-icon ui-icon ui-icon-help"></span>');
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
                    //jQuery(init).find(nameidclassofbuttontodisable).button().button('disable').addClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                    jQuery(init).find(nameidclassofbuttontodisable).button().button('disable').addClass('ButtonHover');
                    OnpageAlter();
                    MakeRadioBtnChecked();
                    $.removeDisble(submitnameformforserilize);
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
                                        $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('disable').addClass('submitbtndisable');
                                        ajaxloaderv2('body');
                                    },
                                });
                                chkajx.done(function (msg) {
                                    $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('enable').removeClass('submitbtndisable');
                                    //$('.ajax_loder').parents('div').remove();
                                    ajaxLoderRemove();
                                    if (msg.success == true) {
                                        //success event
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
                                                    if (fn != undefined) {
                                                        if (fn.CloseRetunFun != null) {
                                                            fn.CloseRetunFun(msg.success);
                                                        }
                                                    }
                                                }
                                            }
                                        });
                                        newDiv.dialog('open');
                                        $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                                        y = false;
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
                                                    if (fn != undefined) {
                                                        if (fn.CloseRetunFun != null) {
                                                            fn.CloseRetunFun(msg.success);
                                                        }
                                                    }
                                                }
                                            }
                                        });
                                        newDiv.dialog('open');
                                        $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                                    }

                                });
                                chkajx.fail(function (xhr, status, error) {
                                    $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('enable').removeClass('submitbtndisable');
                                    //$('.ajax_loder').parents('div').remove();
                                    ajaxLoderRemove();
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
                                $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('disable').addClass('submitbtndisable');
                                ajaxloaderv2('body');
                            },
                        });
                        ajaxdata.done(function (msg) {
                            $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('enable').removeClass('submitbtndisable');
                            // $('.ajax_loder').parents('div').remove();
                            ajaxLoderRemove();
                            var htmltag = "";
                            if (msg.success == true) {
                                var newDiv = $(document.createElement('div'));
                                for (var i = 0; i < msg.responseText.length; i++) {
                                    htmltag += '<span class="ajax-action-class-container"><span style="float:left;display:block"><i class="fa fa-fw fa-3x fa-check-circle-o ajax-success-icon" aria-hidden="true"></i></span><span class="ajax-action-text"> ' + msg.responseText[i] + '</span></span>';
                                }
                                newDiv.html(htmltag);
                                newDiv.dialog({
                                    autoOpen: false,
                                    title: "Information",
                                    height: 400, width: 250, modal: true,
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
                                            jQuery(init).find('select').empty().append("<option value=0 selected=true>-Select-</option>").selectmenu().selectmenu("refresh");
                                            jQuery(init).find('input').empty();
                                            jQuery(init).find('textarea').empty();
                                            //jQuery(init).find(nameidclassofbuttontodisable).button().button('enable').removeClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                                            jQuery(init).find(nameidclassofbuttontodisable).button().button('enable').removeClass('ButtonHover');
                                            jQuery(init).find(nameofthelookuptable).find('tr td').parent().remove();
                                            newDiv.dialog("close");
                                            jQuery(maindailog).dialog("close");
                                            if (fn != undefined) {
                                                if (fn.CloseRetunFun != null) {
                                                    fn.CloseRetunFun();
                                                }
                                            }
                                        }
                                    }
                                });
                                newDiv.dialog('open');
                                $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                            } else {
                                var newDiv = $(document.createElement('div'));
                                for (var i = 0; i < msg.responseText.length; i++) {
                                    htmltag += '<span class="ajax-action-class-container"><span style="float:left;display:block"><i class="fa fa-fw fa-3x fa-exclamation-circle ajax-error-icon" aria-hidden="true"></i></span><span class="ajax-action-text"> ' + msg.responseText[i] + '</span></span>';
                                }
                                newDiv.html(htmltag);
                                newDiv.dialog({
                                    autoOpen: false,
                                    title: "Error",
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
                        ajaxdata.fail(function (jqXHR, textStatus) {
                            $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('enable').removeClass('submitbtndisable');
                            //$('.ajax_loder').parents('div').remove();
                            ajaxLoderRemove();
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

        $.fn.P2BDeleteModalDialog = function (deleteurl, deletedata, deletemessage, deletesuccessmessage, deleteerrormessage, gridreloadname, height, width, fn) {
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
                        var allDataForAPI = JSON.stringify(P2bBindAllDataForDelete(deletedata));
                        deleteajaxdata = $.ajax({
                            url: deleteurl,
                            method: 'POST',
                            contentType: 'application/json',
                            data: allDataForAPI,
                            beforeSend: function () {
                                // alert('hiihihihihi');
                                $('.ui-dialog-buttonpane').find('button:contains("Confirm")').button().button('disable').addClass('submitbtndisable');
                                ajaxloaderv2('body');

                            },
                        });
                        deleteajaxdata.done(function (msg) {
                            $('.ui-dialog-buttonpane').find('button:contains("Confirm")').button().button('enable').removeClass('submitbtndisable');
                            // $('.ajax_loder').parents('div').remove();
                            ajaxLoderRemove();
                            var htmltag = "";
                            if (msg.MessageCode == 200) {
                                var newDiv2 = $(document.createElement('div'));
                                htmltag += '<span class="ajax-action-class-container"><span style="float:left;display:block"><i class="fa fa-fw fa-3x fa-check-circle-o ajax-success-icon" aria-hidden="true"></i></span><span class="ajax-action-text"> ' + msg.Message + '</span></span>';

                                newDiv2.html(htmltag);
                                newDiv2.dialog({
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
                                            newDiv2.dialog("close");
                                            $(newDiv2).remove();
                                            jQuery(newDiv).dialog("close");
                                            $(newDiv).remove();

                                        }
                                    }
                                });
                                newDiv2.dialog('open');
                                $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                            } else {
                                var newDiv2 = $(document.createElement('div'));
                                htmltag += '<span class="ajax-action-class-container"><span style="float:left;display:block"><i class="fa fa-fw fa-3x fa-exclamation-circle ajax-error-icon" aria-hidden="true"></i></span><span class="ajax-action-text"> ' + msg.Message + '</span></span>';

                                newDiv2.html(htmltag);
                                newDiv2.dialog({
                                    autoOpen: false,
                                    title: "Error",
                                    height: 250, width: 400, modal: true,
                                    buttons: {
                                        Ok: function (e) {
                                            newDiv2.dialog("close");
                                            newDiv2.remove();
                                        }
                                    }
                                });
                                newDiv2.dialog('open');
                                $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                            }

                        });
                        deleteajaxdata.fail(function (jqXHR, textStatus) {
                            $('.ui-dialog-buttonpane').find('button:contains("Confirm")').button().button('enable').removeClass('submitbtndisable');
                            // $('.ajax_loder').parents('div').remove();
                            ajaxLoderRemove();
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
            var originalObjectData;
            var DTObjectData;
            var editajaxdata, editajaxopenloaddata, init;
            //$('select').removeAttr('style');
            var OldIds = [];
            var NewIds = [];
            var olddata = [];
            nameidclassofbuttontodisable = '.popup-content-icon-edit,.popup-content-icon-remove,.popup-content-icon-view';
            var confrm, flag;
            init = jQuery(this);
            var maindialog = jQuery(init).dialog({
                autoOpen: false,
                height: height,
                width: width,
                modal: true,
                //iconButtons: [{
                //    text: "Help",
                //    icon: "ui-icon-help",
                //    click: function (e) {
                //        helpfun("edit", "" + submitnameformforserilize.slice("4") + "");
                //    }
                //}, {
                //    text: "Authorization",
                //    icon:"ui-icon-taskcomplete"
                //}],
                //create: function () {
                //    var helpButton = $('<button>', {
                //        text: 'Help',
                //        //class: 'titlebar-btn',
                //        click: function () {
                //            alert('Help button clicked!');
                //        }
                //    });
                //    $(this).closest('.ui-dialog').find('.ui-dialog-titlebar').append(helpButton);
                //},
                closeOnEscape: false,
                title: maindialogtitle,
                beforeClose: function (e) {
                    RemoveLookupTableElement(forwardserializedata); RemoveErrTag();
                    
                    //if (nameofthelookuptable != null && nameofthelookuptable != "") {
                    // NewIds = CountTableIdsVal(nameofthelookuptable);
                    //}
                    //var olddata = get1DArray(OldIds);
                    ////bypass checking remaining
                    //NewIds = [];
                    //olddata = [];
                    //if (NewIds.length == 0 && olddata.length == 0) {
                    // flag = true;
                    //}
                    //$.each(olddata, function (i, k) {
                    // if (NewIds.indexOf(k) > -1) {
                    // flag = true;
                    // } else {
                    // flag = false;
                    // }
                    //});
                    //if (flag == true && NewIds.length == olddata.length) {
                    // NewIds = [];
                    // olddata = [];
                    // OldIds = [];
                    // jQuery(init).find("select").empty().append("<option selected=true>-Select-</option>").removeClass("ui-selectmenu-text").selectmenu().selectmenu("refresh");
                    // jQuery(init).find('input').empty().removeAttr('style').removeAttr('readonly');
                    // jQuery(init).find('textarea').empty().removeAttr('style').removeAttr('readonly');
                    // jQuery(init).find(nameidclassofbuttontodisable).button('enable').removeClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                    // jQuery(init).find(nameofthelookuptable).find('tr td').parent().remove();
                    // return true;
                    //} else {
                    // var confrm = confirm("Save Modified Data Or Else It Will Be Discarded");
                    // //Ok-true
                    // //cancel-false
                    // if (confrm == false) {
                    // return false;
                    // } else {
                    // NewIds = [];
                    // olddata = [];
                    // OldIds = [];
                    // jQuery(init).find("select").empty().append("<option selected=true>-Select-</option>").removeClass("ui-selectmenu-text").selectmenu().selectmenu("refresh");
                    // jQuery(init).find('input').empty().removeAttr('style').removeAttr('readonly');
                    // jQuery(init).find('textarea').empty().removeAttr('style').removeAttr('readonly');
                    // jQuery(init).find(nameidclassofbuttontodisable).button('enable').removeClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                    // jQuery(init).find(nameofthelookuptable).find('tr td').parent().remove();
                    // return true;
                    // }
                    //}

                    //e.preventDefault();
                },
                open: function (event, ui) {
                    $.CheckSessionExitance();
                    $('.ui-dialog-titlebar-help').html('<span class="ui-button-icon ui-icon ui-icon-help"></span>');
                    //var helpButton = $('<button>', {
                    //    title:'Authorization',
                    //    html: '<span class="ui-icon ui-icon-person"></span>',
                    //    //html: '<span class="ui-icon ui-icon-document"></span>',
                    //    class: 'ui-button ui-corner-all ui-widget ui-dialog-titlebar-close ui-dialog-titlebar-person',
                    //    click: function () {
                    //        P2bCreateAuthorizationDialog({ FormName: forwardserializedata, OriginalData: originalObjectData, ModifiedData: DTObjectData });
                    //    }
                    //});
                    //$(this).closest('.ui-dialog').find('.ui-dialog-titlebar').append(helpButton);
                    NewIds = [];
                    olddata = [];
                    OldIds = [];
                    var allDataForAPI = JSON.stringify(P2bBindAllDataForEdit(opendataforward));
                    editajaxopenloaddata = $.ajax({
                        url: openurl,
                        method: 'POST',
                        contentType: 'application/json',
                        data: allDataForAPI,
                        //data: { data: opendataforward },
                        beforeSend: function () {
                            ajaxloaderv2('body');
                        },
                        complete: function () {
                            // $('.ajax_loder').parents('div').remove();
                            ajaxLoderRemove();
                        }
                    });
                    editajaxopenloaddata.done(function (value) {
                        //if (value[1] != undefined || value[1] != null) {
                        // if (value[1][0] != undefined || value[1][0] != null) {
                        // $.each(value[1][0], function (i, k) {
                        // if (k != null) {
                        // OldIds.push(k);
                        // }
                        // });
                        // }
                        //}
                        if (typeof returndatafunction === 'function') {
                            if (value && value.Data) {
                                if (!singleData['AutoAuthorization'] && !singleData['Action'].includes('C')) {
                                    DTObjectData = value.Data.DTData;
                                    originalObjectData = value.Data.OriginalData;
                                    //if (DTObjectData['Action'] === 'C') {
                                    //    P2bCreateAuthorizationDialog({ FormName: forwardserializedata, OriginalData: originalObjectData, ModifiedData: DTObjectData, Forwarddata: forwarddata });
                                    //}
                                } else {
                                    DTObjectData = value.Data.OriginalData;
                                }
                            } 
                            returndatafunction(value);
                        } else {
                            console.log('returnfun is not define');
                        }
                    });
                    OnpageAlter();

                    //jQuery(init).find(nameidclassofbuttontodisable).button().button('disable').addClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                    jQuery(init).find(nameidclassofbuttontodisable).button().button('disable').addClass('ButtonHover');
                },
                buttons: {
                    Submit: function (e) {
                        var x = PerformValidations(forwardserializedata);
                        var y = true;
                        if (fn != undefined) {
                            if (fn.validurl != null && x == true) {
                                var allDataForAPI = $.param(P2bBindAllDataForCreate(forwardserializedata, nameofthelookuptable));
                                var chkajx = $.ajax({
                                    url: fn.validurl,
                                    method: "POST",
                                    async: false,
                                    data: allDataForAPI,
                                    beforeSend: function () {
                                        $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('disable').addClass('submitbtndisable');
                                        ajaxloaderv2('body');
                                    },
                                });
                                chkajx.done(function (msg) {
                                    $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('enable').removeClass('submitbtndisable');
                                    // $('.ajax_loder').parents('div').remove();
                                    ajaxLoderRemove();
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
                                chkajx.fail(function (xhr, status, error) {
                                    $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('enable').removeClass('submitbtndisable');
                                    // $('.ajax_loder').parents('div').remove();
                                    ajaxLoderRemove();
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
                            beforeClose: function (e) {
                                $(newDiv2).remove(); RemoveErrTag();
                            },
                            modal: true,
                            buttons: {
                                Confirm: function () {
                                    var allDataForAPI = JSON.stringify(P2bBindAllDataForEditSave(forwardserializedata, forwarddata, nameofthelookuptable, originalObjectData, DTObjectData));
                                    editajaxdata = $.ajax({
                                        url: editurl,
                                        method: 'POST',
                                        contentType: 'application/json',
                                        data: allDataForAPI,
                                        //data: $(forwardserializedata).serialize() + '&data=' + forwarddata + '',
                                        beforeSend: function () {
                                            // alert('hiihihihihi');
                                            $('.ui-dialog-buttonpane').find('button:contains("Confirm")').button().button('disable').addClass('submitbtndisable');
                                            ajaxloaderv2('body');

                                        },
                                    });
                                    editajaxdata.done(function (msg) {
                                        var htmltag = "";
                                        $('.ui-dialog-buttonpane').find('button:contains("Confirm")').button().button('enable').removeClass('submitbtndisable');
                                        // $('.ajax_loder').parents('div').remove();
                                        ajaxLoderRemove();
                                        if (msg.MessageCode == 200) {
                                            var newDiv = $(document.createElement('div'));
                                            htmltag += '<span class="ajax-action-class-container"><span style="float:left;display:block"><i class="fa fa-fw fa-3x fa-check-circle-o ajax-success-icon" aria-hidden="true"></i></span><span class="ajax-action-text"> ' + msg.Message + '</span></span>';

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
                                                        if (fn != undefined) {
                                                            if (fn.datatablename != null) {
                                                                var table = $(fn.datatablename).DataTable();
                                                                table.ajax.reload();
                                                            }
                                                        }
                                                        //NewIds = [];
                                                        //olddata = [];
                                                        //OldIds = [];
                                                        RemoveLookupTableElement(forwardserializedata);
                                                        jQuery(init).find('select').empty().append("<option value=0 selected=true>-Select-</option>").selectmenu().selectmenu().selectmenu("refresh");
                                                        jQuery(init).find('input').empty();
                                                        jQuery(init).find('textarea').empty();
                                                        //jQuery(init).find(nameidclassofbuttontodisable).button().button('enable').removeClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                                                        jQuery(init).find(nameidclassofbuttontodisable).button().button('enable').removeClass('ButtonHover');
                                                        jQuery(init).find(nameofthelookuptable).find('tr td').parent().remove();
                                                        newDiv.dialog("close");
                                                        jQuery(maindialog).dialog("close");
                                                        jQuery(newDiv2).dialog("close");

                                                    }
                                                }
                                            });
                                            newDiv.dialog('open');
                                        } else {
                                            var newDiv = $(document.createElement('div'));
                                            htmltag += '<span class="ajax-action-class-container"><span style="float:left;display:block"><i class="fa fa-fw fa-3x fa-exclamation-circle ajax-error-icon" aria-hidden="true"></i></span><span class="ajax-action-text"> ' + msg.Message + '</span></span>';

                                            newDiv.html(htmltag);
                                            newDiv.dialog({
                                                autoOpen: false,
                                                title: "Error",
                                                height: 250, width: 400, modal: true,
                                                buttons: {
                                                    Ok: function (e) {
                                                        newDiv.dialog("close");
                                                        newDiv.remove();
                                                        newDiv2.dialog('close');
                                                        $(newDiv).remove();

                                                    }
                                                }
                                            });
                                            newDiv.dialog('open');
                                        }
                                        $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                                    });
                                    editajaxdata.fail(function (jqXHR, textStatus) {
                                        $('.ui-dialog-buttonpane').find('button:contains("Confirm")').button().button('enable').removeClass('submitbtndisable');
                                        // $('.ajax_loder').parents('div').remove();
                                        ajaxLoderRemove();
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
            var submitIcon = $('.ui-icon-disk');
            if (submitIcon.length === 0) {
                submitIcon = '<span class="ui-icon ui-icon-disk"></span>';
            }
            $('.ui-dialog-buttonpane').find('button:contains("Submit")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend(submitIcon);
            //$('.ui-dialog-buttonpane').find('button:contains("Submit")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-disk"></span>');
            $('.ui-dialog-buttonpane').find('button:contains("Cancel")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-closethick"></span>');
        };
        $.fn.P2BViewModalDialog = function (openurl, opendataforward, maindialogtitle, nameofthelookuptable, nameidclassofbuttontodisable, height, width, idorclassofautobtn, authoriseurl, autho_id, autho_action, autho_data, editmessage, editerrormessage, gridreloadname, returndatafunction) {
            var viewajaxopenloaddata;
            //$('select').removeAttr('style');
            var old_data_class = ['.olddiv', '.oldlookup', '.olddrop', '.oldradio'];
            nameidclassofbuttontodisable = ".popup-content-icon-create,.popup-content-icon-edit,.popup-content-icon-remove,.popup-content-icon-lookup,.popup-content-icon-view";
            var init = jQuery(this);
            autho_data = false;
            var maindialog = jQuery(init).dialog({
                autoOpen: false,
                height: height,
                width: width,
                modal: true,
                beforeClose: function () {
                    RemoveLookupTableElement(init);
                    RemoveErrTag();
                    jQuery(init).find("select").selectmenu('enable').empty().append("<option selected=true value=0>-Select-</option>").removeClass("ui-selectmenu-text").selectmenu().selectmenu("refresh");
                    jQuery(init).find('input').prop('disabled', false).removeAttr('style').removeAttr('readonly');
                    jQuery(init).find('textarea').prop('disabled', false).removeAttr('style').removeAttr('readonly');
                    jQuery(init).find('' + nameofthelookuptable + ' tr td').attr('disabled', 'disabled');
                    jQuery(init).find(nameofthelookuptable).find('tr td').parent().remove();
                    //jQuery(init).find(nameidclassofbuttontodisable).button().button('enable').removeClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                    jQuery(init).find(nameidclassofbuttontodisable).button().button('enable').removeClass('ButtonHover');
                    $.each(old_data_class, function (i, k) { $(init).find(k).remove(); });
                },
                closeOnEscape: false,
                title: maindialogtitle,
                open: function (event, ui) {
                    $.CheckSessionExitance();
                    $('.ui-dialog-titlebar-help').html('<span class="ui-button-icon ui-icon ui-icon-help"></span>');
                    var allDataForAPI = JSON.stringify(P2bBindAllDataForEdit(opendataforward));
                    viewajaxopenloaddata = $.ajax({
                        url: openurl,
                        method: 'POST',
                        contentType: 'application/json',
                        data: allDataForAPI,
                        beforeSend: function () {
                            ajaxloaderv2('body');
                        },
                        complete: function () {
                            // $('.ajax_loder').parents('div').remove();
                            ajaxLoderRemove();
                        }
                    });
                    viewajaxopenloaddata.done(function (value) {
                        returndatafunction(value);
                    });
                    jQuery(this).find("select").selectmenu('disable').addClass("ui-selectmenu-text");
                    jQuery(this).find('input').prop('disabled', true).css("background-color", "rgba(241, 241, 241, 0.66)");
                    jQuery(this).find('textarea').prop('disabled', true).css("background-color", "rgba(241, 241, 241, 0.66)");
                    jQuery(this).find('' + nameofthelookuptable + ' tr td').attr('disabled', 'disabled');
                    //jQuery(this).find(nameidclassofbuttontodisable).button().button('disable').addClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                    jQuery(this).find(nameidclassofbuttontodisable).button().button('disable').addClass('ButtonHover');
                    //jQuery(this).find(nameidclassofbuttontodisable).button().button('disable');
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
                                $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('disable').addClass('submitbtndisable');
                                ajaxloaderv2('body');

                            },
                        });
                        editajaxdata.done(function (msg) {
                            RemoveLookupTableElement(init);
                            $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('enable').removeClass('submitbtndisable');
                            //$('.ajax_loder').parents('div').remove();
                            ajaxLoderRemove();
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
                                        jQuery(init).find("select").selectmenu('enable').empty().append("<option selected=true value=0>-Select-</option>").removeClass("ui-selectmenu-text").selectmenu().selectmenu("refresh");
                                        jQuery(init).find('input').prop('disabled', false).removeAttr('style').removeAttr('readonly');
                                        jQuery(init).find('textarea').prop('disabled', false).removeAttr('style').removeAttr('readonly');
                                        jQuery(init).find('' + nameofthelookuptable + ' tr td').attr('disabled', 'disabled');
                                        jQuery(init).find(nameofthelookuptable).find('tr td').parent().remove();
                                        //jQuery(init).find(nameidclassofbuttontodisable).button().button('enable').removeClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                                        jQuery(init).find(nameidclassofbuttontodisable).button().button('enable').removeClass('ButtonHover');
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
                            $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('enable').removeClass('submitbtndisable');
                            //$('.ajax_loder').parents('div').remove();
                            ajaxLoderRemove();
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
            $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('disable').addClass('submitbtndisable');
            $('.ui-dialog-buttonpane').find('button:contains("Submit")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-disk"></span>');
            $('.ui-dialog-buttonpane').find('button:contains("Cancel")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-closethick"></span>');
        };
        //---------------Utilites---Funtion----------------------------------------------------------------
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
            });
        }
        function responseXmlDataManipulation(xml) {
            return xml;
        };
        //$.fn.onClickGrid = function (gridname, url1, url2) {
        //    $(this).on("click", function () {
        //        if ($(this).hasClass('auto_active')) {
        //            $(gridname).P2BGrid.onclickChangeUrl(gridname, url2, true);
        //        } else {
        //            $(gridname).P2BGrid.onclickChangeUrl(gridname, url1, false);
        //        }
        //    });
        //};
        $.fn.makeDisable = function (buttonsmakedisble, gridname, url1) {
            var authoBtn = $(this);
            $(this).on("click", function (e) {
                if (!$(this).hasClass('auto_active')) {
                    var yesNoDialog = $(document.createElement('div'));
                    var smallDialogMsg = `<div style="display:flex; align-items:center; justify-content:center; flex-direction:column;">
                        <span class="ui-icon ui-icon-person" style="display:block; margin-right:10px;"></span>
                        <b>You Are In Authorized Mode.</b>
                        <p> Do You Want To Continue?</p>
                    </div>`;

                    yesNoDialog.html(smallDialogMsg);
                    yesNoDialog.dialog({
                        autoOpen: false,
                        title: `Authorization Mode!`,
                        height: 170,
                        width: 305,
                        closeOnEscape: false,
                        beforeClose: function (e) {
                            $(yesNoDialog).remove(); RemoveErrTag();
                        },
                        modal: true,
                        buttons: {
                            Yes: function () {
                                $(buttonsmakedisble).button('disable');
                                $(authoBtn).toggleClass('auto_active');
                                if ($(authoBtn).hasClass('auto_active')) {
                                    AuthorizeStatus = IsAuthorisedObject['Yes'];
                                    if (AuthorizeStatus && !singleData['Action'].includes('C')) {
                                        $('#Edit').button("enable");
                                    }
                                    $(gridname).P2BGrid.onclickChangeUrl(gridname, url1, true);
                                    $('.Form_Bg,.page_content').css('background-color', 'lightyellow');
                                    //$('.Form_Bg,.page_content').css('background-color', '#003366');
                                    //$('.Form_Bg,.page_content').css('background-color', '#003366');
                                    //$('.Form_Bg,.page_content').css('background-color', ' #c3eabf ');
                                    //$('.Form_Bg,.page_content').css('background-color', '  #cde793 ');
                                    //$('.Form_Bg,.page_content').css('background-color', '#eb96c0');
                                } else {
                                    $('.ui-dialog-content').find('.old_data_div').remove();
                                    if (AuthorizeStatus && !singleData['Action'].includes('C')) {
                                        $('#Edit').button("disable");
                                    } else {
                                        $(buttonsmakedisble).button('enable');
                                    }
                                    $(gridname).P2BGrid.onclickChangeUrl(gridname, url1, false);
                                    AuthorizeStatus = IsAuthorisedObject['No'];
                                }
                                $(this).dialog('close');
                            },
                            No: function () {
                                $(this).dialog('close');
                            }
                        }
                    });

                    $(yesNoDialog).dialog('open');
                    $('.ui-dialog-buttonpane').find('button:contains("Yes")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                    $('.ui-dialog-buttonpane').find('button:contains("No")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-closethick"></span>');

                } else {
                    $(authoBtn).toggleClass('auto_active');
                    if (AuthorizeStatus && !singleData['Action'].includes('C')) {
                        $(buttonsmakedisble).button('disable');
                    } else {
                        $(buttonsmakedisble).button('enable');
                    }
                    $(gridname).P2BGrid.onclickChangeUrl(gridname, url1, false);
                    AuthorizeStatus = IsAuthorisedObject['No'];
                    $('.Form_Bg,.page_content').css('background-color', '');
                }

                //$(buttonsmakedisble).button('disable');
                //$(this).toggleClass('auto_active');
                //if ($(this).hasClass('auto_active')) {
                //    AuthorizeStatus = IsAuthorisedObject['Yes'];
                //    if (AuthorizeStatus && !singleData['Action'].includes('C')) {
                //        $('#Edit').button("enable");
                //    }
                //    $(gridname).P2BGrid.onclickChangeUrl(gridname, url2, true);
                //} else {
                //    $('.ui-dialog-content').find('.old_data_div').remove();
                //    if (AuthorizeStatus && !singleData['Action'].includes('C')) {
                //        $('#Edit').button("disable");
                //    } else {
                //        $(buttonsmakedisble).button('enable');
                //    }
                //    $(gridname).P2BGrid.onclickChangeUrl(gridname, url1, false);
                //    AuthorizeStatus = IsAuthorisedObject['No'];
                //}
            });
        };
        //$.fn.makeDisable = function (buttonsmakedisble) {
        //    var note_html = '<span class="parent_span" style="display:inline-block;position:relative"><span style="color:red">*</span><span>Note:The Text Which Are In<span class="child_span_color"></span>&nbsp;&nbsp;&nbsp;&nbsp; Color Is Old Data </span></span>';
        //    if (!$(this).hasClass('auto_active')) {
        //        $(buttonsmakedisble).button("enable");
        //    }
        //    $(this).on("click", function (e) {
        //        $(this).toggleClass('auto_active');
        //        if ($(this).hasClass('auto_active')) {
        //            $('<div class="old_data_div">' + note_html + '</div>').appendTo('.ui-dialog-content');
        //        } else {
        //            $('.ui-dialog-content').find('.old_data_div').remove();
        //        }
        //        if (!$(buttonsmakedisble).button("option", "disabled")) {
        //            $(buttonsmakedisble).button("disable").css("background-color", "rgba(241, 241, 241, 0.66)");;
        //        } else {
        //            $(buttonsmakedisble).button("enable").css("background-color", "");
        //        }
        //    });
        //};
        $.fn.makeDisable1 = function (buttonsmakedisble) {
            var note_html = '<span class="parent_span" style="display:inline-block;position:relative"><span style="color:red"></span></span>';
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
            $('<div style="width:100%;height:100%;"><i class="fa fa-circle-o-notch fa-spin fa-5x fa-fw ajax_loder" style="color: rgba(27, 187, 173, 0.93);" aria-hidden="true"></i></div>').appendTo(cameform);
        };
        function ajaxloaderv2(cameform) {
            $('<div style="width: 100%;height: 100%;position: fixed;top: 0;left: 0;background-color: #0000;z-index:998;"><i class="fa fa-circle-o-notch fa-spin fa-5x fa-fw ajax_loder" style="color: rgba(27, 187, 173, 0.93);z-index:999" aria-hidden="true"></i></div>').appendTo(cameform);
        };
        $.fn.StickNote = function (old_val) {
            var objtype = $(this).attr('type') || $(this)[0].nodeName.toLowerCase();
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
            $('input[type="radio"]').val([false]).button().button("refresh");
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
            var regx = /^[a-zA-Z0-9 ]+$/g;
            if ($(control).val().length != 0 && !(regx.test($(control).val()))) {
                ShowTextBoxErrorMsg('AlphaNum', control, "Enter Numbers or Alphabet..!");
                return false;
            } else {
                return 'no-error';
            }
        };
        var PersonName = function (control) {
            // $(control).val($(control).val().toUpperCase());
            var regx = /^[a-zA-Z0-9-'_ ,()]+$/g;
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
            var id = $(control).attr('id') + "_error";
            $("<div class='error' id=" + id + ">" + msg + "</div>").appendTo("body").css({ left: pos.left + w + 10, top: pos.top });
            if (comingfrom == 'AlphaNum') {
                $(control).val($(control).val().replace(/[^a-zA-Z0-9]/g, ''));
            } else if (comingfrom == 'maxlength') {
                var len = $(control).data('maxlenght');
                $(control).val($(control).val().substring(0, len));
            } else if (comingfrom == 'PersonName') {
                $(control).val($(control).val().replace(/[^a-zA-Z0-9-'_ ,()]/g, ''));
            }
            $(control).off('click change selectmenuchange').on('click change selectmenuchange', function (e) {
                var a = $("#" + $(this).attr('id') + "_error");
                if (a) a.remove();

            });
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
                ShowErrorMsg(comingfrom, "Minimum " + count + " Uppercase Character Required");
                return InValid;
            } else {
                return valid;
            }
        };
        var minlowchars = function (comingfrom, count) {
            var valid = true;
            var InValid = false;
            if ($(comingfrom).val().replace(/[^a-z]/g, "").length < count) {
                ShowErrorMsg(comingfrom, "Minimum " + count + " LowerCase Character Required");
                return InValid;
            } else {
                return valid;
            }
        };
        var minnonos = function (comingfrom, count) {
            var valid = true;
            var InValid = false;
            if ($(comingfrom).val().replace(/\D/g, "").length < count) {
                ShowErrorMsg(comingfrom, "Minimum " + count + " Number Required");
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
                ShowErrorMsg(comingfrom, "Minimum " + count + " Special Required");
                return InValid;
            } else {
                return valid;
            }
        };
        var ShowErrorMsg = function (comingfrom, msg) {
            var pos = $(comingfrom).offset();
            var h = $(comingfrom).height();
            var w = $(comingfrom).width();
            var id = $(comingfrom).attr('id') + "_error";

            if (!$('div.error')[0]) {
                $("<div class='error' id=" + id + ">" + msg + "</div>")
                    .appendTo("body")
                    .css({
                        left: pos.left + 2,
                        top: pos.top + h + 10
                    });
            } else {
                $(comingfrom).off('click change selectmenuchange').on('click change selectmenuchange', function (e) {
                    var a = $("#" + $(this).attr('id') + "_error");
                    if (a) a.remove();

                });
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

            $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('disable');

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

                        $('.ui-dialog-buttonpane button:contains("Submit")').button().button("enable");

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
            $('#' + lookuptable + ' tr td input').each(function () {
                x = $(this).val();
                if (x != "" && x != undefined) {
                    abcd.push(parseInt(x));
                }
            });
            return abcd;
        };
        $.LocalStorageHelper = function () {
            if (typeof (Storage) !== "undefined") {
                if (arguments.length != 2) {
                    return localStorage.getItem(arguments[0]);
                } else {
                    localStorage.setItem(arguments[0], arguments[1]);
                }
            } else {
                alert("webstorage Avaliable");

            }
        }
        $.fn.P2BPartialCreateModalDialog = function (url, maindialogtitle, state, submiturl, submitnameformforserilize, savemessage, errormessage, gridreloadname, height, width, tableName, ControlName, event, classoridoftheonwhichpopupderived, nameidclassofbuttontodisable, returnfunctiondata, fn) {
            jQuery(this).trigger('reset');
            var ajaxdata, loadpartialajax, init;
            init = jQuery(this);
            nameidclassofbuttontodisable = '.popup-content-icon-edit,.popup-content-icon-remove,.popup-content-icon-view';
            var maindailog = jQuery(this).dialog({
                autoOpen: false,
                height: height,
                width: width,
                modal: true,
                closeOnEscape: false,
                title: maindialogtitle,
                beforeClose: function () {
                    jQuery(init).find("select").detach();
                    jQuery(submitnameformforserilize).find(nameidclassofbuttontodisable).button().button('enable').removeClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                    jQuery(init).remove(); RemoveErrTag();
                },
                open: function (event, ui) {
                    $.CheckSessionExitance();
                    jQuery.ajax({
                        url: url,
                        method: "GET",
                        beforeSend: function () {
                            ajaxloader(init);
                            $(init).find("div .ajax_loder").show();
                        },
                        success: function (result, status, xhr) {
                            //done: function (result, status, xhr) {
                            showajaxeroor(xhr, init);
                            $(init).find("div .ajax_loder").hide();
                            $(init).html(result);
                            //MakeRadioBtnChecked();
                            OnEnterFocusNext();
                            AlterBtnType();
                            if (typeof returnfunctiondata === "function") {
                                var data = [0, 0];
                                returnfunctiondata(data);
                            }
                            //$(submitnameformforserilize).find(nameidclassofbuttontodisable).button().button('disable').button().button("refresh").addClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                            $(submitnameformforserilize).find(nameidclassofbuttontodisable).button().button('disable').button().button("refresh").addClass('ButtonHover');
                        },
                        error: function (xhr, status, error) {
                            showajaxeroor(xhr, init);
                            //$(init).find("div .ajax_loder").show();
                            ajaxLoderRemove();
                        },
                        complete: function () {
                            //$(init).find("div .ajax_loder").hide();
                            ajaxLoderRemove(init);
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
                                        $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('disable').addClass('submitbtndisable');
                                        ajaxloaderv2('body');
                                    },
                                });
                                //chkajx.success(function (msg) {
                                chkajx.done(function (msg) {
                                    $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('enable').removeClass('submitbtndisable');
                                    //$('.ajax_loder').parents('div').remove();
                                    ajaxLoderRemove();
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
                                //chkajx.error(function (xhr, status, error) {
                                chkajx.fail(function (xhr, status, error) {
                                    $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('enable').removeClass('submitbtndisable');
                                    //$('.ajax_loder').parents('div').remove();
                                    ajaxLoderRemove();
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
                        var allDataForAPI = JSON.stringify(P2bBindAllDataForCreate(submitnameformforserilize, tableName));
                        ajaxdata = $.ajax({
                            url: submiturl,
                            method: "POST",
                            contentType: 'application/json',
                            beforeSend: function () {
                                $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('disable').addClass('submitbtndisable');
                                ajaxloaderv2('body');
                            },
                            data: allDataForAPI,
                            //data: $(submitnameformforserilize).serialize() + '&data=' + forwarddata + '',
                        });
                        //ajaxdata.success(function (msg) {
                        ajaxdata.done(function (msg) {
                            $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('enable').removeClass('submitbtndisable');
                            //$('.ajax_loder').parents('div').remove();
                            ajaxLoderRemove();
                            var htmltag = '';
                            if (msg.MessageCode == 200) {
                                var newDiv = $(document.createElement('div'));
                                htmltag += '<span class="ajax-action-class-container"><span style="float:left;display:block"><i class="fa fa-fw fa-3x fa-check-circle-o ajax-success-icon" aria-hidden="true"></i></span><span class="ajax-action-text"> ' + msg.Message + '</span></span>';

                                newDiv.html(htmltag);
                                newDiv.dialog({
                                    autoOpen: false,
                                    title: "Information",
                                    closeOnEscape: false,
                                    height: 150, width: 250,
                                    modal: true,
                                    buttons: {
                                        Ok: function () {
                                            var data = [];
                                            data.push(msg.Data);
                                            returnfunctiondata(data);
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
                            } else {
                                var newDiv = $(document.createElement('div'));
                                htmltag += '<span class="ajax-action-class-container"><span style="float:left;display:block"><i class="fa fa-fw fa-3x fa-exclamation-circle ajax-error-icon" aria-hidden="true"></i></span><span class="ajax-action-text"> ' + msg.Message + '</span></span>';

                                newDiv.html(htmltag);
                                newDiv.dialog({
                                    autoOpen: false,
                                    title: "Error",
                                    height: 250, width: 400, modal: true,
                                    buttons: {
                                        Ok: function (e) {
                                            newDiv.dialog("close");
                                            newDiv.remove();
                                            newDiv2.dialog('close');
                                            $(newDiv).remove();

                                        }
                                    }
                                });
                                newDiv.dialog('open');
                            }
                            $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                        });
                        ajaxdata.fail(function (jqXHR, textStatus) {
                            $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('enable').removeClass('submitbtndisable');
                            //$('.ajax_loder').parents('div').remove();
                            ajaxLoderRemove();
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
            var originalObjectDataForPartialEdit;
            var DTObjectDataForPartialEdit;
            var editajaxdata, editajaxopenloaddata, init;
            init = jQuery(this);
            var maindialog = jQuery(this).dialog({
                autoOpen: false,
                height: height,
                width: width,
                modal: true,
                closeOnEscape: false,
                beforeClose: function () {
                    //jQuery(forwardserializedata).find(nameidclassofbuttontodisable).button().button('enable').removeClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)").button().button("refresh");
                    jQuery(forwardserializedata).find(nameidclassofbuttontodisable).button().button('enable').removeClass('ButtonHover').button().button("refresh");
                    jQuery(init).remove(); RemoveErrTag();
                },
                title: maindialogtitle,
                open: function (event, ui) {
                    $.CheckSessionExitance();
                    function assigndata() {
                        var allDataForAPI = JSON.stringify(P2bBindAllDataForEdit(opendataforward));
                        editajaxopenloaddata = $.ajax({
                            url: openurl,
                            method: 'POST',
                            contentType: 'application/json',
                            data: allDataForAPI,
                        });
                        editajaxopenloaddata.done(function (value) {
                            DTObjectDataForPartialEdit = value.Data.DTData;
                            originalObjectDataForPartialEdit = value.Data.OriginalData;
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
                            //$(init).find("div .ajax_loder").hide();
                            ajaxLoderRemove(init);
                            $(init).html(result);
                            assigndata();
                            OnEnterFocusNext();
                            AlterBtnType();
                            //$(forwardserializedata).find(nameidclassofbuttontodisable).button().button('disable').button().button("refresh").addClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                            $(forwardserializedata).find(nameidclassofbuttontodisable).button().button('disable').button().button("refresh").addClass('ButtonHover');
                        },
                        error: function (xhr, status, error) {
                            showajaxeroor(xhr, init);
                            $(init).find("div .ajax_loder").show();
                        },
                        complete: function () {
                            //$(init).find("div .ajax_loder").hide();
                            ajaxLoderRemove(init);

                        }
                    });
                },
                buttons: {
                    Submit: function () {
                        var x = PerformValidations(forwardserializedata);
                        var y = true;
                        if (fn != undefined) {
                            if (fn.validurl != null && x == true) {
                                var allDataForAPI = $.param(P2bBindAllDataForEditSave(forwardserializedata, { Id: forwarddata }));
                                var chkajx = $.ajax({
                                    url: fn.validurl,
                                    method: "POST",
                                    contentType: 'application/json',
                                    async: false,
                                    data: allDataForAPI,
                                    //data: $(forwardserializedata).serialize(),
                                    beforeSend: function () {
                                        $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('disable').addClass('submitbtndisable');
                                        ajaxloaderv2('body');
                                    },
                                });
                                chkajx.done(function (msg) {
                                    $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('enable').removeClass('submitbtndisable');
                                    //$('.ajax_loder').parents('div').remove();
                                    ajaxLoderRemove(init);

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
                                chkajx.fail(function (xhr, status, error) {
                                    $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('enable').removeClass('submitbtndisable');
                                    //$('.ajax_loder').parents('div').remove();
                                    ajaxLoderRemove();
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
                        var allDataForAPI = JSON.stringify(P2bBindAllDataForEditSave(forwardserializedata, forwarddata, classoridoftheonwhichpopupderived, originalObjectDataForPartialEdit, DTObjectDataForPartialEdit));
                        editajaxdata = $.ajax({
                            url: editurl,
                            method: "POST",
                            contentType: 'application/json',
                            beforeSend: function () {
                                $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('disable').addClass('submitbtndisable');
                                ajaxloaderv2('body');
                            },
                            data: allDataForAPI,
                            //data: $(forwardserializedata).serialize() + '&data=' + forwarddata + '',
                        });
                        editajaxdata.done(function (msg) {
                            $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('enable').removeClass('submitbtndisable');
                            //$('.ajax_loder').parents('div').remove();
                            ajaxLoderRemove();
                            var htmltag = "";
                            if (msg.MessageCode == 200) {
                                var newDiv = $(document.createElement('div'));
                                htmltag += '<span class="ajax-action-class-container"><span style="float:left;display:block"><i class="fa fa-fw fa-3x fa-check-circle-o ajax-success-icon" aria-hidden="true"></i></span><span class="ajax-action-text"> ' + msg.Message + '</span></span>';

                                newDiv.html(htmltag);
                                var data = [];
                                data.push(msg.Id);
                                data.push(msg.Val);
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
                                                jQuery(classoridoftheonwhichpopupderived).find('' + nameclassidofinlinelookup + ' tr td:contains(' + data[0] + ')').parent('tr.selectedtr').remove();
                                                var flagForOriginalOrEdit = compareDataAndSetFlag(forwardserializedata, originalObjectDataForPartialEdit, DTObjectDataForPartialEdit);
                                                $(nameclassidofinlinelookup).P2BLookUpEncapsulate(nameclassidofinlinelookup, nameofthelist_inlinelookuptable, data[0], data[1], nameoftable_inlinelookuptable, 'Edit', multiple_allowed_or_not, flagForOriginalOrEdit);
                                                //$(nameclassidofinlinelookup).P2BLookUpEncapsulate(nameclassidofinlinelookup, nameofthelist_inlinelookuptable, data[0], data[1], nameoftable_inlinelookuptable, nameidclassofbuttontodisable, multiple_allowed_or_not,'E');
                                            }
                                            else {
                                                alert("Parameter is Not Passed Properly.");
                                            }
                                            newDiv.dialog("close");
                                            newDiv.remove();
                                            //jQuery(forwardserializedata).find(nameidclassofbuttontodisable).button().button('enable').removeClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                                            jQuery(forwardserializedata).find(nameidclassofbuttontodisable).button().button('enable').removeClass('ButtonHover');
                                            jQuery(maindialog).dialog("close");
                                            jQuery(init).remove();
                                        }
                                    }
                                });
                                newDiv.dialog('open');
                            } else {
                                var newDiv = $(document.createElement('div'));
                                htmltag += '<span class="ajax-action-class-container"><span style="float:left;display:block"><i class="fa fa-fw fa-3x fa-exclamation-circle ajax-error-icon" aria-hidden="true"></i></span><span class="ajax-action-text"> ' + msg.Message + '</span></span>';

                                newDiv.html(htmltag);
                                newDiv.dialog({
                                    autoOpen: false,
                                    title: "Error",
                                    height: 250, width: 400, modal: true,
                                    buttons: {
                                        Ok: function (e) {
                                            newDiv.dialog("close");
                                            newDiv.remove();

                                        }
                                    }
                                });
                                newDiv.dialog('open');
                            }
                            $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                        });
                        editajaxdata.fail(function (jqXHR, textStatus) {
                            $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('enable').removeClass('submitbtndisable');
                            //$('.ajax_loder').parents('div').remove();
                            ajaxLoderRemove();
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
                                            // jQuery(gridreloadname).trigger('reloadGrid');
                                        }
                                        newDiv.dialog("close");
                                        //jQuery(forwardserializedata).find(nameidclassofbuttontodisable).button().button('enable').removeClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                                        jQuery(forwardserializedata).find(nameidclassofbuttontodisable).button().button('enable').removeClass('ButtonHover');
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
        $.fn.P2BPartialDeleteModalDialog = function (deleteurl, deletedata, forwarddata, deletemessage, deletesuccessmessage, deleteerrormessage, selectfield, optionvalue, height, width, classoridoftheonwhichpopupderived, nameclassidofinlinelookup, btndisable, fn) {
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
                    newDiv.remove(); RemoveErrTag();
                },
                buttons: {
                    Confirm: function () {
                        if (deleteurl && deleteurl !== '') {
                            var allDataForAPI = JSON.stringify(P2bBindAllDataForDelete(deletedata));
                            $.ajax({
                                url: deleteurl,
                                method: "POST",
                                beforeSend: function () {
                                    $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('disable').addClass('submitbtndisable');
                                    ajaxloaderv2('body');
                                },
                                data: allDataForAPI,
                                success: function () {
                                    ajaxLoderRemove();
                                },
                                error: function () {
                                    ajaxLoderRemove();
                                }
                            });
                        }
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
                            width: 200,
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
                                        $.each(deletedata, function () {
                                            jQuery(classoridoftheonwhichpopupderived).find('' + nameclassidofinlinelookup).find("tr.selectedtr").remove();
                                        });
                                    }
                                    var nameidclassofbuttontodisable = ".popup-content-icon-edit,.popup-content-icon-remove,.popup-content-icon-view";
                                    var a = $(nameclassidofinlinelookup).parents('div').next('.icon-row').find(nameidclassofbuttontodisable);

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
        $.fn.P2BLookUpModal = function (lookupurl, lookupdata, LookupDiv, lookuptitle, tablename, nameoftable, dataontable, nameofthelist, classoridoftheonwhichpopupderived, multipleallowedornot, nameidclassofbuttontodisable, setnameofthelookupbyppage, pagename) {
            //----------HTML-------------Example---------------
            // <div class="dialog">
            // <div title="LookUp Data">
            // <div class="LookupDiv"></div>
            // </div>
            // <div id="pageNavPosition">

            // </div>
            //</div>
            var init = jQuery(this);
            //init.trigger('reset');
            var data;
            nameidclassofbuttontodisable = ".popup-content-icon-edit,.popup-content-icon-remove,.popup-content-icon-view";
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
                        pagerHtml += '<span onclick="' + pagerName + '.next();" class="pg-normal"><span style="cursor:pointer;">Next &#x21E8;</span></span>';

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
                        if (oldPageAnchor) {
                            oldPageAnchor.className = 'pg-normal';
                        }

                        this.currentPage = pageNumber;
                        var newPageAnchor = document.getElementById('pg' + this.currentPage);
                        if (newPageAnchor) {
                            newPageAnchor.className = 'pg-selected';
                        }

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

                        var inline = jQuery(tablename).find('td').eq(0).text();
                        if (multipleallowedornot == 'A') {
                            if (target.className != "selectedtr") {
                                target.className = "selectedtr";
                                
                                jQuery(tablename).append('<tr tabindex="-1"><td class="' + nameofthelist + '"><input type="text" name="' + nameofthelist + '" value="' + data[0] + '"/>A</td><td>' + data[1] + '</td></tr>').insertAfter($(this).closest('tr')).TableOnRowsClick(nameoftable);
                                //jQuery(tablename).append('<tr tabindex="-1"><td><input type="text" name="' + nameofthelist + '" value="' + data[0] + '"/>' + data[0] + '</td><td>' + data[1] + '</td></tr>').insertAfter($(this).closest('tr')).TableOnRowsClick(nameoftable);
                                //jQuery(classoridoftheonwhichpopupderived).find('#' + nameofthelist + '').parents('div').next('.icon-row').find(nameidclassofbuttontodisable).button().button('enable').removeClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                                jQuery(classoridoftheonwhichpopupderived).find('#' + nameofthelist + '').parents('div').next('.icon-row').find(nameidclassofbuttontodisable).button().button('enable').removeClass('ButtonHover');
                                jQuery(tablename + ' tr td').addClass("selectedtr");
                                setTimeout(function () {
                                    jQuery(tablename + ' tr td ').removeClass("selectedtr")
                                }, 500);
                            }
                            else {
                                target.className = "";
                                jQuery(tablename + ' tr td:contains(' + data[0] + ')').closest('tr').remove();
                                if (jQuery(tablename + ' tr').closest('tr').length == 1) {
                                    //jQuery(classoridoftheonwhichpopupderived).find('#' + nameofthelist + '').parents('div').next('.icon-row').find(nameidclassofbuttontodisable).button().button('disable').addClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                                    jQuery(classoridoftheonwhichpopupderived).find('#' + nameofthelist + '').parents('div').next('.icon-row').find(nameidclassofbuttontodisable).button().button('disable').addClass('ButtonHover');
                                }
                            }
                        }
                        else if (multipleallowedornot == 'N') {
                            if (count >= 2) {
                                if (target.className == "selectedtr") {
                                    target.className = "";
                                    jQuery(tablename + ' tr td:contains(' + data[0] + ')').closest('tr').remove();
                                    //jQuery(classoridoftheonwhichpopupderived).find('#' + nameofthelist + '').parents('div').next('.icon-row').find(nameidclassofbuttontodisable).button().button('disable').addClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                                    jQuery(classoridoftheonwhichpopupderived).find('#' + nameofthelist + '').parents('div').next('.icon-row').find(nameidclassofbuttontodisable).button().button('disable').addClass('ButtonHover');
                                }
                                else if (target.className != "selectedtr" && inline == data[0]) {
                                    target.className = "";
                                    jQuery(tablename + ' tr td:contains(' + data[0] + ')').closest('tr').remove();
                                    //jQuery(classoridoftheonwhichpopupderived).find('#' + nameofthelist + '').parents('div').next('.icon-row').find(nameidclassofbuttontodisable).button().button('disable').addClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                                    jQuery(classoridoftheonwhichpopupderived).find('#' + nameofthelist + '').parents('div').next('.icon-row').find(nameidclassofbuttontodisable).button().button('disable').addClass('ButtonHover');
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

                                    //jQuery('' + tablename + ' tr:last').after('<tr tabindex="-1"><td><input type="text" name="' + nameofthelist + '" value="' + firstdataparameter + '"/>' + firstdataparameter + '</td><td>' + seconddataparameter + '</td></tr>');
                                    jQuery('' + tablename + ' tr:last').after('<tr tabindex="-1"><td class="' + nameofthelist + '"><input type="text" name="' + nameofthelist + '" value="' + data[0] + '"/>A</td><td>' + data[1] + '</td></tr>').TableOnRowsClick(nameoftable);
                                    //jQuery('' + tablename + ' tr:last').after('<tr tabindex="-1"><td><input type="text" name="' + nameofthelist + '" value="' + data[0] + '"/>' + data[0] + '</td><td>' + data[1] + '</td></tr>').TableOnRowsClick(nameoftable);
                                    //jQuery(classoridoftheonwhichpopupderived).find('#' + nameofthelist + '').parents('div').next('.icon-row').find(nameidclassofbuttontodisable).button().button('enable').removeClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                                    jQuery(classoridoftheonwhichpopupderived).find('#' + nameofthelist + '').parents('div').next('.icon-row').find(nameidclassofbuttontodisable).button().button('enable').removeClass('ButtonHover');
                                    jQuery(tablename + ' tr td').addClass("selectedtr");
                                    setTimeout(function () {
                                        jQuery(tablename + ' tr td ').removeClass("selectedtr")
                                    }, 500);
                                }
                                else {
                                    target.className = "";
                                    jQuery(tablename + ' tr td:contains(' + data[0] + ')').closest('tr').remove();
                                    if (count == 1) {
                                        //jQuery(classoridoftheonwhichpopupderived).find('#' + nameofthelist + '').parents('div').next('.icon-row').find(nameidclassofbuttontodisable).button().button('disable').addClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                                        jQuery(classoridoftheonwhichpopupderived).find('#' + nameofthelist + '').parents('div').next('.icon-row').find(nameidclassofbuttontodisable).button().button('disable').addClass('ButtonHover');
                                    }
                                }
                            }
                        }
                    }
                }
            };
            var searchon = function () {
                $('#lookup-search').on('keyup', function (e) {
                    if (e.which == 13) {
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
                    }
                });
                $('#selectall_lookuptable').on('click', function (e) {
                    if (multipleallowedornot == 'A') {
                        var $tr = $('#lookup_table tr:gt(0)').toArray();
                        var data = [];
                        for (var i = 0; i < $tr.length; i++) {
                            var td_arry = $($tr[i]).find('td').toArray();
                            for (var j = 0; j < 2; j++) {
                                data.push(td_arry[j].innerHTML);
                            }
                            jQuery('' + tablename + ' tr:last').after('<tr tabindex="-1"><td class="' + nameofthelist + '"><input type="text" name="' + nameofthelist + '" value="' + data[0] + '"/>A</td><td>' + data[1] + '</td></tr>').TableOnRowsClick(nameoftable);
                            //jQuery('' + tablename + ' tr:last').after('<tr tabindex="-1"><td><input type="text" name="' + nameofthelist + '" value="' + data[0] + '"/>' + data[0] + '</td><td>' + data[1] + '</td></tr>').TableOnRowsClick(nameoftable);
                            data = [];
                            //jQuery(classoridoftheonwhichpopupderived).find('#' + nameofthelist + '').parents('div').next('.icon-row').find(nameidclassofbuttontodisable).button().button('enable').removeClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                            jQuery(classoridoftheonwhichpopupderived).find('#' + nameofthelist + '').parents('div').next('.icon-row').find(nameidclassofbuttontodisable).button().button('enable').removeClass('ButtonHover');
                        }
                    }
                    e.preventDefault();
                });
            };

            var lookupajaxdata;
            var dia = jQuery(this).dialog({
                autoOpen: false,
                width: '404',
                height: 'auto',
                modal: true,
                title: lookuptitle,
                closeOnEscape: false,
                beforeClose: function () {
                    jQuery(dia).dialog('destroy').hide(); RemoveErrTag();
                    jQuery('#' + pagename + '').empty();
                },
                open: function (event, ui) {
                    jQuery('.' + LookupDiv + '').empty();
                    jQuery('#' + pagename + '').empty();
                    $.CheckSessionExitance();
                    var ids = CountTableIds(tablename);
                    var allDataForAPI = JSON.stringify(P2bBindAllDataForLookUp(ids));
                    lookupajaxdata = $.ajax({
                        url: lookupurl,
                        type: 'POST',
                        cache: false,
                        contentType: 'application/json',
                        datatype: 'json',
                        data: allDataForAPI,
                        //data: JSON.stringify([null, null, ids]),
                        //data: JSON.stringify([{ Id: ids }]),
                        beforeSend: function () {
                            ajaxloader(init);
                            $(init).find("div .ajax_loder").show();
                        }, 

                    });
                    lookupajaxdata.done(function (successdata) {
                        var htmltag = '<div style="margin: 0px auto;width:100%;padding: 1px 0px 5px;float:left"><input type="text" style="width:97%"; placeholder="Search" id="lookup-search" /></div><table class="lookuptable" id="' + setnameofthelookupbyppage + '"><tr><th>Sr.No</th><th><a id="selectall_lookuptable" style="float:left;padding: 1px 0px;" title="Select All">SelectAll</a>Description</th></tr>'
                        if (successdata.Data) {
                            var allListData = successdata.Data.FullDetailsList;
                            if (allListData) {
                                const FullDetailsKey = Object.keys(allListData).find(item => item.includes('_FullDetails'));
                                for (let i = 0; i < allListData.Id.length; i++) {
                                    htmltag += '<tr tabindex="-1">' +
                                        '<td>' + allListData.Id[i] + '</td>' +
                                        '<td>' + allListData[FullDetailsKey][i] + '</td>' +
                                        '</tr>'
                                }
                            }
                            htmltag += '</table>';
                            jQuery('.' + LookupDiv + '').html(htmltag);
                            clickon();
                            pageon();
                            searchon();
                            $(init).find("div .ajax_loder").hide();
                        }
                    });
                },
                buttons: {
                    Ok: function () {
                        jQuery(dia).dialog('destroy').hide();
                        jQuery('#' + pagename + '').empty();
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
            var table = document.getElementById(tabledvalues);
            var tbody = table.getElementsByTagName("tbody")[0];
            var all_child = $(tbody)[0].children;
            tbody.onclick = function (e) {
                e = e || window.event;
                var target = e.srcElement || e.target;
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
                    data.push(jQuery(this).find('input').val());
                    //data.push(jQuery(this).find('td').eq(indexvalueoftable).text());
                }
            });
            return data;
        };
        $.fn.P2BLookUpEncapsulate = function (tablename, nameofthelist, firstdataparameter, seconddataparameter, nameoftable,
            nameofthebtndisable, multiple_allowed_or_not,createOrEditActionFlag) {
            if (firstdataparameter == undefined || firstdataparameter == '' || firstdataparameter == null || seconddataparameter == undefined || seconddataparameter == '' || seconddataparameter == null) {
                return null;
            }
            else {
                //Value For Para multiple_allowed_or_not
                //A = Allowed;
                //N= Not Allowed;
                if (nameofthebtndisable && nameofthebtndisable.toUpperCase() != 'View'.toUpperCase()) {
                    nameofthebtndisable = ".popup-content-icon-edit,.popup-content-icon-remove,.popup-content-icon-view";
                } else {
                    nameofthebtndisable = "";
                }
                if (multiple_allowed_or_not == 'N') {

                    var count = $('' + tablename + ' tbody')[0].childNodes.length;
                    if (count == 3) {
                        $('<div></div>').P2BMessageModalDialog('ui-icon-alert', 'Only One ReCord Allowed...!');
                    } else {
                        if (firstdataparameter.length && seconddataparameter.length > 1) {
                        }
                        $(tablename).parents('div').next('.icon-row').find(nameofthebtndisable).button().button('enable');
                        jQuery('' + tablename + ' tr:last').after('<tr tabindex="-1"><td class="' + nameofthelist + '"><input type="text" name="' + nameofthelist + '" value="' + firstdataparameter + '"/>' + createOrEditActionFlag + '</td><td>' + seconddataparameter + '</td></tr>');
                        //jQuery('' + tablename + ' tr:last').after('<tr tabindex="-1"><td><input type="text" name="' + nameofthelist + '" value="' + firstdataparameter + '"/>' + firstdataparameter + '</td><td>' + seconddataparameter + '</td></tr>');
                    }
                } else if (multiple_allowed_or_not == 'A') {
                    if (typeof seconddataparameter == 'string') {
                        $(tablename).parents('div').next('.icon-row').find(nameofthebtndisable).button().button('enable');
                        jQuery('' + tablename + ' tr:last').after('<tr tabindex="-1"><td class="' + nameofthelist + '"><input type="text" name="' + nameofthelist + '" value="' + firstdataparameter + '"/>' + createOrEditActionFlag + '</td><td>' + seconddataparameter + '</td></tr>');
                        //jQuery('' + tablename + ' tr:last').after('<tr tabindex="-1"><td><input type="text" name="' + nameofthelist + '" value="' + firstdataparameter + '"/>' + firstdataparameter + '</td><td>' + seconddataparameter + '</td></tr>');
                        LookupTableSelectedRowOnClick(nameofthelist);
                    }
                    if (typeof firstdataparameter == 'object' && typeof seconddataparameter == 'object') {
                        $(tablename).parents('div').next('.icon-row').find(nameofthebtndisable).button().button('enable');
                        for (var i = 0; i < firstdataparameter.length && seconddataparameter.length; i++) {
                            jQuery('' + tablename + ' tr:last').after('<tr tabindex="-1"><td class="' + nameofthelist + '"><input type="text" name="' + nameofthelist + '" value="' + firstdataparameter[i] + '"/>' + createOrEditActionFlag + '</td><td>' + seconddataparameter[i] + '</td></tr>');
                            //jQuery('' + tablename + ' tr:last').after('<tr tabindex="-1"><td><input type="text" name="' + nameofthelist + '" value="' + firstdataparameter[i] + '"/>' + firstdataparameter[i] + '</td><td>' + seconddataparameter[i] + '</td></tr>');
                            if (i == 0) {
                                LookupTableSelectedRowOnClick(nameofthelist);
                            }
                        }
                    }
                }
                var count = $('' + tablename + ' tbody')[0].childNodes.length;
                if (count == 3) {
                    LookupTableSelectedRowOnClick(nameofthelist);
                }
            }
        };
        $.fn.P2BSelectMenuAppend = function (url, forwardata, forwardata2, drop2) {
            var init = jQuery(this);
            var w = $(init).css('width');
            var htm = '<option style=' + w + ' value=0 selected=true>-Select-</option>';
            jQuery(init).empty().append(htm).selectmenu().selectmenu("refresh");
            $.post(url, { data: forwardata, data2: forwardata2 }, function (data) {
                $.each(data, function (i, k) {
                    jQuery(init).append($('<option>', {
                        value: k.Value,
                        text: k.Text,
                        selected: k.Selected
                    }));
                });
                jQuery(init).selectmenu().selectmenu("refresh").selectmenu("menuWidget").css("height", "100px");
            });
            // $("<span class='DropdownCode'>" + forwardata + "</span>").insertAfter(init);
            jQuery(drop2).empty().append(htm).selectmenu().selectmenu("refresh");
        };
        ///new
        $.fn.P2BSelectMenuAppend1 = function (url, forwardata, forwardata2, drop2) {
            var init = jQuery(this);
            var w = $(init).css('width');
            var htm = '<option style=' + w + ' value="" selected=true>-Select-</option>';
            jQuery(init).empty().append(htm).selectmenu().selectmenu("refresh");
            $.post(url, { data: forwardata, data2: forwardata2 }, function (data) {
                $.each(data, function (i, k) {

                    jQuery(init).append($('<option>', {
                        value: k.Value,
                        text: k.Text,
                        selected: k.Selected
                    }));
                });
                jQuery(init).selectmenu().selectmenu("refresh").selectmenu("menuWidget").css("height", "100px");
            });
            // $("<span class='DropdownCode'>" + forwardata + "</span>").insertAfter(init);
            jQuery(drop2).empty().append(htm).selectmenu().selectmenu("refresh");
        };

        $.fn.P2BSelectMenuOnChange = function (onchangeevent, url, filter_drop, data2) {
            var init = jQuery(this);
            jQuery(init).off(onchangeevent).on(onchangeevent, function () {
                jQuery(filter_drop).empty().append("<option value=0 selected=true>-Select-</option>").selectmenu().selectmenu("refresh");
                var value = jQuery(init).val();
                if (value != 0) {
                    $.post(url, { data: value, data2: data2 }, function (data) {
                        $.each(data, function (i, k) {
                            jQuery(filter_drop).append($('<option>', {
                                value: k.Value,
                                text: k.Text,
                                selected: k.Selected
                            }));
                            jQuery(filter_drop).selectmenu().selectmenu("refresh").selectmenu("menuWidget").css({ "height": "auto" });
                        });
                    });
                } else {
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

        $.fn.P2BMessageModalDialog1 = function (ui_icon, Message) {
            var newDivma = $(document.createElement('div'));
            var htmltag = '<p><span class="ui-icon ' + ui_icon + '" style="float:left;margin-right:10px"></span>' + Message + '';
            htmltag += '</p>';
            newDivma.html(htmltag);
            newDivma.dialog({
                autoOpen: false,
                title: "Information",
                height: 500, width: 700, modal: true,
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

                $(transcation_div).find('button').hide();
                $(transcation_div).find('div').hide();
                $(init).hide();

                var heading = $(init).find('tr th:eq(1)').text();

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

                if (x != undefined) {
                    $(x).checked(true);
                }
            });
        };
        $.fn.AddDataToTranscation = function (tablename, lookupurl, NoOfRecordToShow, setnameofthelookupbyppage, LookupDiv, lookuppagename, pagename) {
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
                        pagerHtml += '<span onclick="' + pagerName + '.next();" class="pg-normal"><span style="cursor:pointer;">Next &#x21E8;</span></span>';

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
                    $.CheckSessionExitance();
                    $(".ui-dialog-titlebar-close", ui.dialog | ui).on('click', function () {
                        jQuery(init).dialog('destroy').hide();
                    });
                    var ids = CountTranscationTableIds(tablename);

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
                        jQuery('.' + LookupDiv + '').html(htmltag);
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
                jQuery(IDofinputsearch).on('keypress', function (e) {
                    if (e.keyCode == 13) {

                        var $rows = $('#' + init.attr('id') + ' tr');
                        var value = jQuery(this).val().toUpperCase().toLowerCase();
                        if (value == '') {
                            $rows.removeClass('table-div-hide');
                            return false;
                        }
                        $rows.each(function (index) {
                            if (index !== 0) {
                                var $row = $(this);
                                var column2 = $row.find("td").eq(2).text().toUpperCase().toLowerCase();
                                if ((column2.indexOf
                                    (value) > -1)) {
                                    $row.removeClass('table-div-hide');
                                }
                                else {
                                    $row.addClass('table-div-hide');
                                }
                            }
                        });
                    } else if (e.keyCode == 8) {
                        var $rows = $('#' + init.attr('id') + ' tr');
                        $rows.removeClass('table-div-hide');
                    }
                });
            };
            function checkboselection() {

                $(IDofCheckbox).off('click').on('click', function (e) {
                    var b = init.attr('id');
                    single = single || false;
                    if (single == false) {
                        if (this.checked) {
                            $('#' + b + ' .case').each(function (i, k) {
                                var element = $(k);
                                var parent = element.parent('td').parent('tr');
                                if (!parent.hasClass('table-div-hide')) {
                                    if (!element.is('checked')) {
                                        element.attr('checked', 'checked');
                                        element.prop('checked', true);
                                        parent.addClass('selectedtr');
                                    }
                                }

                            });
                        } else {

                            $('#' + b + ' .case').each(function (i, k) {
                                var ele = $(k);
                                var parent = ele.parent('td').parent('tr');
                                if (!parent.hasClass('table-div-hide')) {
                                    parent.removeClass('selectedtr');
                                    ele.removeAttr('checked');
                                    ele.prop('checked', false);
                                }

                            });
                        }
                    } else {
                        return false;
                    }
                });
                //
                $(document).on('click', '#' + init.attr('id') + ' .case', function (e) {
                    single = single || false;
                    if (single == false) {
                        if (this.checked) {
                            var value_checked = jQuery('#' + init.attr('id') + ' ' + '.case:checked').parent('td').parent('tr');
                            value_checked.addClass('selectedtr');
                        }
                        else {
                            var value_unchecked = jQuery(this).parent('td').parent('tr');
                            value_unchecked.removeClass('selectedtr');
                        }
                    } else {
                        if (this.checked) {
                            var value_checked = jQuery('#' + init.attr('id') + ' ' + '.case:checked').parent('td').parent('tr');
                            value_checked.addClass('selectedtr');
                            $('#' + init.attr('id') + ' ' + '.case:checked').not(this).prop('checked', false).parent('td').parents('tr:eq(0)').removeClass('selectedtr');
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
                //ajaxloaderv2('body');
                var dataload = jQuery.ajax({
                    url: urldatatoload,
                    type: 'POST',
                    cache: false,
                    data: { data: datatoserverside.toString() }
                });
                dataload.done(function (data) {
                    //ajaxLoderRemove();		

                    $.each(data, function (j, l) {
                        if (l != null && l.data != null) {
                            $('#' + l.tablename + '').parent('div.transactiondiv').parents('div').show();
                            $('#' + l.tablename + '>tbody>tr:gt(0)').remove();

                            $.each(l.data, function (i, k) {
                                jQuery('#' + l.tablename + ' tr:last').after('<tr tabindex="1"><td><input type="checkbox" class="case" name=' + l.tablename + ' value=' + k.code + ' /></td><td style="display:none;">' + k.code + '</td><td>' + k.value + '</td></tr>');
                            });
                        } else {

                            //if (l.responseText != null) {
                            //    alert(l.responseText);
                            //}

                            //code by vinayak
                            if (data.responseText != null) {
                                $('#' + data.data + '').parent('div.transactiondiv').parents('div').show();
                                $('#' + data.data + '>tbody>tr:gt(0)').remove();
                                alert(data.responseText);
                                data.preventDefault();
                            }
                        }
                    });
                });
            }
            searchon();
            checkboselection();
        };
        var TranscationTableCheckboxCount = function (table) {
            return $(table).find('input:checkbox:gt(0):checked').length;
        };
        $.ApplyFilter = function (url, inputname, formname, callback) {
            formname = formname || "#FormTransactionForm";
            $.ajax({
                method: "POST",
                url: url,
                data: $(formname).serialize(),
                success: function (data) {
                    if (typeof data != "string" && data != "") {
                        var val;
                        if ($(inputname).val()) {
                            val = $(inputname).val() + "," + data;
                        } else {
                            val = data;
                        }
                        $(inputname).val(val);
                        if (typeof callback === "function") {
                            callback.call();
                        }
                    } else {
                        $(inputname).val("");
                        if (typeof callback === "function") {
                            callback.call();
                        }
                    }
                }
            });
        };
        $.fn.DynamicSelectMenuAppend = function (Url, SelectType) {
            var init = jQuery(this);
            jQuery("#" + $(this).attr('id') + "").empty().append("<option value=0 selected=true>-Select-</option>").selectmenu().selectmenu("refresh");
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
                        jQuery('#' + data.SelectlistType + '').selectmenu().selectmenu("refresh").selectmenu("menuWidget").addClass("overflow");
                    });
                }
            });
        };
        $.fn.DynamicSelectMenuOnChange = function (Url) {
            var init = jQuery(this);
            jQuery(init).on("selectmenuchange", function () {
                var value = jQuery("#" + $(this).attr('id') + "").val();
                var SelectType = $(this).attr('id').substr(0, $(this).attr('id').indexOf("_"));

                if (value != 0) {

                    //  if (SelectType != "Company") {
                    $.post(Url, { data: value, data2: SelectType }, function (data) {
                        if (data.SelectlistType != null) {
                            $('#' + data.SelectlistType + '').parent().show();
                            jQuery("#" + data.SelectlistType + "").empty().append("<option value=0 selected=true>-Select-</option>").selectmenu().selectmenu("refresh");
                            $.each(data.selectlist, function (i, k) {
                                jQuery('#' + data.SelectlistType + '').append($('<option>', {
                                    value: k.Value,
                                    text: k.Text,
                                    selected: k.Selected
                                }));
                                jQuery('#' + data.SelectlistType + '').selectmenu().selectmenu("refresh").selectmenu("menuWidget").addClass("overflow");
                            });
                        }
                    });
                    // }
                    //else {
                    //    $.post(Url, { data: value, data2: SelectType }, function (data) {
                    //       
                    //        $.each(data, function (i, j) {
                    //            
                    //            if (j.SelectlistType != null) {
                    //                $('#' + j.SelectlistType + '').parent().show();
                    //                jQuery("#" + j.SelectlistType + "").empty().append("<option value=0 selected=true>-Select-</option>").selectmenu().selectmenu("refresh");
                    //                $.each(j.selectlist, function (i, k) {
                    //                    jQuery('#' + j.SelectlistType + '').append($('<option>', {
                    //                        value: k.Value,
                    //                        text: k.Text,
                    //                        selected: k.Selected
                    //                    }));
                    //                    jQuery('#' + j.SelectlistType + '').selectmenu().selectmenu("refresh").selectmenu("menuWidget").addClass("overflow");
                    //                });
                    //            }
                    //        });
                    //    });
                    //}
                } else {
                    jQuery('#' + data.SelectlistType + '').empty()
                        .append("<option value=0 selected=true>-Select-</option>")
                        .selectmenu().selectmenu("refresh")
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
                LocalStorageId: null,
                onEditClick: false
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
                rowNum: 100,
                rowList: [10, 20, 30, 100],
                pager: obj.inlinePager,
                sortname: obj.SortName,
                viewrecords: true,
                sortorder: "asc",
                caption: obj.Caption,
                afterSaveCell: function (data) {
                    var a = $(this).getRowData(data);
                    $(this).InlineGridEdittedData.SetData(a);
                },
                //search: {
                // odata: ['equal']
                //},
                width: obj.width,
                height: obj.height,
                editurl: "clientArray",
                //beforeEditCell: function (rowid, cellname, value, iRow, iCol) {
                // //$("#inline_jqgrid").editCell(iRow, iCol,true);
                // //var a = $("#jqGrid").getRowData(iRow);

                // //if (a[obj.CheckCol] == "1") {

                // //}
                // // here identify row based on rowid
                // // if the row should not be editable than simply make the cells noneditable using
                // //editCell(iRow, iCol, false);
                // //jQuery("#inline_jqgrid").jqGrid("restoreCell", iRow, iCol);

                //},
                beforeSelectRow: function (rowid, e) {
                    var temp = "true";
                    if (obj.onEditClick == true) {
                        temp = $.LocalStorageHelper(obj.LocalStorageId);
                    }
                    var $td = $(e.target).closest("td"), iCol = $.jgrid.getCellIndex($td[0]);
                    var a = $(table).getRowData(rowid);
                    $td.addClass('not-editable-cell');
                    for (var i = 0; i < obj.EditableCol.length; i++) {
                        if (a[obj.CheckCol] == "true" && obj.ColModel.indexOf(obj.EditableCol[i]) + 1 == iCol && temp == "true") {
                            $td.removeClass('not-editable-cell');
                        }
                        else {
                            if (typeof a["FormulaType"] !== "undefined") {
                                if (a["FormulaType"].toUpperCase() == "NONSTANDARDFORMULA" && (iCol == 8 || iCol == 4)) { $td.removeClass('not-editable-cell'); }
                            }
                        }
                    }
                },
                gridComplete: function () {
                    //var id = jQuery(this).jqGrid().getDataIDs(), a = [];
                    //$.each(id, function (i, k) {
                    // var a = $("#inline_jqgrid").getRowData(k);
                    // if (a[obj.CheckCol] == "1") {
                    // //var b = $("#inline_jqgrid").jqGrid('getColProp', obj.EditableCol[0]);

                    // }
                    //});
                    //$.each(a, function (i, k) {
                    // if (k[obj.CheckCol] == "true") {
                    // //for (var j = 0; j < obj.EditableCol.length; j++) {
                    // // var b = $("#inline_jqgrid").jqGrid('getColProp', obj.EditableCol[i]);

                    // //}
                    // }
                    //});

                    //var a = $(this).jqGrid('getColProp', obj.EditableCol[i]);
                    //if (obj.CheckCol != null) {
                    // //jQuery(tablename).jqGrid('getRowData', id, value);
                    // //var x=$(this).jqGrid()
                    //}
                    //a.editable = true;
                    //}
                    //var a = $(this).jqGrid('getColProp', obj.EditableCol[i]);
                    //if (obj.CheckCol != null) {
                    // //jQuery(tablename).jqGrid('getRowData', id, value);
                    // //var x=$(this).jqGrid()
                    //}
                    //a.editable = true;
                    //}
                }
            });
            this.jqGrid('navGrid', obj.inlinePager, { edit: false, add: false, del: false });
        };
        $.GetGridSelctedvalue = function (Gridname, col) {
            //alert("sdasd");
            var a = jQuery(Gridname).jqGrid('getGridParam', 'selarrrow');
            if (a.length != 0) {
                var selected_ids = [];
                for (var i = 0; i < a.length; i++) {
                    if (col != undefined) {
                        selected_ids.push(jQuery(Gridname).jqGrid('getCell', a[i], col));
                    } else {
                        selected_ids.push(jQuery(Gridname).jqGrid('getRowData', a[i]));
                    }
                }
                return selected_ids;
            } else {
                //alert("Select Row..!");
                return 0;
            }
        };
        $.fn.P2BGridDialog = function (fn) {
            var obj = $.extend({
                height: 300,
                width: 500,
                title: null,
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
                submiturl: null,
            }, fn);
            var init = $(this);
            var griddialog = $(this).dialog({
                autoOpen: false,
                height: obj.height,
                width: obj.width,
                modal: false,
                closeOnEscape: false,
                title: obj.title,
                beforeClose: function () {
                    //obj.gridname != null ? $(obj.gridname).trigger("reloadGrid") : true;
                    //obj.returnToGrid != null ? $(obj.returnToGrid).trigger("reloadGrid") : true;
                    // $.FormReset(obj.form)
                    RemoveErrTag();
                    JqGridCheck.select_all = false;
                    $('.ui-dialog-buttonpane button:contains("Submit")').button().button("enable");
                },
                open: function (event, ui) {
                    $.CheckSessionExitance();
                    $('.ui-dialog-titlebar-help').html('<span class="ui-button-icon ui-icon ui-icon-help"></span>');
                    if (obj.submiturl == null) {
                        $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('disable');
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
                        $(obj.returnToGrid).setGridParam({ url: obj.editurl, postData: { id: obj.editdata, filter: obj.filter, searchField: null, searchOper: null, searchString: null }, page: 1 }).trigger("reloadGrid");
                    }
                },
                buttons: {
                    Submit: function () {

                        if (obj.submiturl == null) {
                            return false;
                        }
                        if (obj.gridname != null && obj.gridfunction != null) {
                            var a = $.GetGridSelctedvalue(obj.gridname, "Id");

                            if (a != 0) {
                                obj.forwarddata = a;
                                var lvdataid = $.GetGridSelctedvalue(obj.gridname, "LvEncashId");
                                var Activityid = $.GetGridSelctedvalue(obj.gridname, "ActivityId");
                                $('#Activity_Id').val(Activityid);
                                $('#Activity_Idh').val(Activityid);
                                //var Lvencashid=j
                            }

                            if (JqGridCheck.select_all == true) {
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
                                $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('disable').addClass('submitbtndisable');
                                ajaxloaderv2('body');
                            },
                            data: $(obj.form).serialize() + "&forwarddata=" + obj.forwarddata + "&selected=" + obj.editdata + "&LeaveEncashId=" + lvdataid
                        });
                        ajaxdata.done(function (msg) {
                            $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('enable').removeClass('submitbtndisable');
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
                                            JqGridCheck.select_all = false;
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
                                            // jQuery(obj.gridreloadname).trigger('reloadGrid');
                                            //}
                                            newDiv.dialog("close");
                                            newDiv.remove();
                                            // jQuery(griddialog).dialog("close");
                                        }
                                    }
                                });
                                newDiv.dialog('open');
                                $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                            }

                        });
                        ajaxdata.fail(function (xhr, status, error) {
                            $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('enable').removeClass('submitbtndisable');
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
                                        JqGridCheck.select_all = false;
                                    }
                                }
                            });
                            newDiv.dialog('open');
                            $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                        });
                    },
                    Cancel: function () {
                        jQuery(griddialog).dialog("close");
                        JqGridCheck.select_all = false;
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
            $('' + obj.minimizeid + ' a').on('click', function (e) {
                $(this).find('i').toggleClass('fa-rotate-90');
                $(this).find('a').attr('data-p2bheadertooltip', $(this).find('a').attr('data-p2bheadertooltip') == 'Collapse' ? 'Expand' : 'Collapse');
                $(this).parents('legend').nextAll('div').slideToggle("slow");
                e.preventDefault();
            });
        };
        $.RadioBtnGroup = function (Btn1, Btn2) {
            $("input:radio[name=" + Btn1 + "]").on('change', function (e) {
                $('input:radio[name=' + Btn2 + ']').val([false]).button().button("refresh");
            });
            $("input:radio[name=" + Btn2 + "]").on('change', function (e) {
                $('input:radio[name=' + Btn1 + ']').val([false]).button().button("refresh");
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
                        $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('disable').addClass('submitbtndisable');
                        ajaxloaderv2('body');
                    },
                    success: function (data) {
                        //$('.ajax_loder').parents('div').remove();

                        ajaxLoderRemove();
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
                                        diahtml.remove(); RemoveErrTag();
                                    },
                                    buttons: {
                                        Confirm: function () {
                                            if (obj.gridid != null) {
                                                if (obj.gridid != null) {
                                                    selectedid = $.GetGridSelctedvalue(obj.gridid, "Id");
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
                                        selectedid = $.GetGridSelctedvalue(obj.gridid, "Id");
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
                        $('.ajax_loder').parents('div').remove();
                    }
                });
            }
        };

        /* Added by Rekha 22052018 for empcode parameter */
        $.fn.ProcessConfirmation1 = function (fn) {
            var obj = $.extend({
                confirmurl: null,
                submiturl: null,
                month: null,
                EmpCode: null,
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
                    data: { typeofbtn: typeofbtn, month: $(obj.month).val(), EmpCode: fn.EmpCode },
                    beforeSend: function () {
                        $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('disable').addClass('submitbtndisable');
                        ajaxloaderv2('body');
                    },
                    success: function (data) {
                        //$('.ajax_loder').parents('div').remove();

                        ajaxLoderRemove();
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
                                        diahtml.remove(); RemoveErrTag();
                                    },
                                    buttons: {
                                        Confirm: function () {
                                            if (obj.gridid != null) {
                                                if (obj.gridid != null) {
                                                    selectedid = $.GetGridSelctedvalue(obj.gridid, "Id");
                                                }

                                                $(obj.gridid).setGridParam({ url: obj.submiturl + "?&month=" + $(obj.month).val() + "?&EmpCode=" + fn.EmpCode + "&typeofbtn=" + typeofbtn, postData: { filter: selectedid.toString() }, page: 1 }).trigger("reloadGrid");
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
                                        selectedid = $.GetGridSelctedvalue(obj.gridid, "Id");
                                    }
                                    $(obj.gridid).setGridParam({ url: obj.submiturl + "?&month=" + $(obj.month).val() + "?&EmpCode=" + fn.EmpCode + "&typeofbtn=" + typeofbtn, postData: { filter: selectedid.toString() }, page: 1 }).trigger("reloadGrid");
                                } else {
                                    if (typeof obj.status === 'function') {
                                        obj.status(true);
                                    }
                                }
                            }
                        }
                    },
                    error: function (data) {
                        $('.ajax_loder').parents('div').remove();
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
        var GenrateLink = function (str) {
            var parm = "";
            if (window.location.pathname.split('/').length > $.LocalStorageHelper('LinkLength')) {
                return "../" + str + parm + "";

            } else {
                return "./" + str + parm + "";
            }
        };


        $.fn.FilterDialog = function (fn) {

            var Check_box = "";
            $(document).delegate('#Get_AllEmployee', 'change', function (e) {

                if ($(this).is(':checked')) {
                    Check_box = "check";
                    //alert(Check_box);
                }
                else {
                    Check_box = "uncheck";
                    //alert(Check_box);
                }

            });

            var obj = $.extend({
                width: null,
                height: null,
                title: null,
                htmlurl: null,
                appendat: null,
                returnat: null,
                renderat: null,
                form: "#advancefilter",
                GeoUrl: "Transcation/Get_Geoid",
                PayUrl: "Transcation/Get_Payid",
                FunUrl: "Transcation/Get_Funid",
                hierarchy: false,
            }, fn);
            var init = $(this);
            if (obj.hierarchy == true) {
                obj.GeoUrl = "Transcation/Get_Geoid_h";
                obj.PayUrl = "Transcation/Get_Payid_h";
                obj.FunUrl = "Transcation/Get_Funid_h";
            }
            function funApplyFilter() {
                $('#filter-geo-struct-id').val("");
                $('#filter-pay-struct-id').val("");
                $('#filter-fun-struct-id').val("");
                $.ApplyFilter(GenrateLink(obj.GeoUrl), "#filter-geo-struct-id", "#geo", function () {
                    $.ApplyFilter(GenrateLink(obj.PayUrl), "#filter-pay-struct-id", "#pay", function () {
                        $.ApplyFilter(GenrateLink(obj.FunUrl), "#filter-fun-struct-id", "#fun", function () {
                            var data = {
                                GeoStruct: $('form#advancefilter #filter-geo-struct-id').val() || null,
                                PayStruct: $('form#advancefilter #filter-pay-struct-id').val() || null,
                                FunStruct: $('form#advancefilter #filter-fun-struct-id').val() || null,
                                CheckAll: Check_box || null
                            };
                            alert('Filter Applied..!');
                            obj.returnat(data);
                        });
                    });
                });
            }
            function ajaxfun() {
                var ajx = $.ajax({
                    url: obj.htmlurl,
                    method: "GET",
                });
                ajx.done(function (html) {
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
                            funApplyFilter();
                            e.preventDefault();
                        });
                        $('form#advancefilter').on('click', '#applyfilter', function (e) {
                            funApplyFilter();
                            e.preventDefault();
                        });
                    }
                });
                ajx.fail(function (data) {
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
                onsubmitreturnfunction: null,
                type: null
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
                    jQuery(init).remove(); RemoveErrTag();
                },
                title: obj.title,
                open: function (event, ui) {
                    $.CheckSessionExitance();
                    function assigndata() {
                        var editajaxopenloaddata = $.ajax({
                            url: obj.onloaddataurl,
                            method: 'POST',
                            data: { data: obj.onloaddataid },
                        });
                        editajaxopenloaddata.done(function (value) {
                            if (typeof obj.onloadreturnfunction === "function") { obj.onloadreturnfunction(value); }
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
                            //$(init).find("div .ajax_loder").hide();
                            ajaxLoderRemove(init);
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
                            //$(init).find("div .ajax_loder").hide();
                            ajaxLoderRemove(init);
                        }
                    });
                },
                buttons: {
                    Submit: function () {
                        var x = PerformValidations(obj.form);
                        if (x == true) {
                            if (obj.type == null) {
                                var editajaxdata = $.ajax({
                                    url: obj.submiturl,
                                    method: "POST",
                                    async: true,
                                    data: $(obj.form).serialize() + '&data=' + obj.onloaddataid + '',
                                    beforeSend: function () {
                                        // alert('hiihihihihi');
                                        $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('disable').addClass('submitbtndisable');
                                        ajaxloaderv2('body');

                                    },
                                });
                                editajaxdata.done(function (msg) {
                                    $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('enable').removeClass('submitbtndisable');
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
                                    }

                                });
                                editajaxdata.fail(function (xhr, status, error) {
                                    $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('enable').removeClass('submitbtndisable');
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
                            } else if (obj.type.toUpperCase() == "FILE") {
                                var tempExcel = new FormData($(obj.form));
                                var editajaxdata = $.ajax({
                                    url: obj.submiturl,
                                    method: "POST",
                                    data: tempExcel,
                                    dataType: 'json',
                                    contentType: 'application/json; charset=UTF-8',
                                    beforeSend: function () {
                                        // alert('hiihihihihi');
                                        $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('disable').addClass('submitbtndisable');
                                        ajaxloaderv2('body');

                                    },
                                });
                                editajaxdata.done(function (msg) {
                                    $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('enable').removeClass('submitbtndisable');
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
                                    }

                                });
                                editajaxdata.fail(function (xhr, status, error) {
                                    $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('enable').removeClass('submitbtndisable');
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
                appendToTextbox: null,
                lookupurl: null,
                NoOfRecordToShow: null,
                setnameofthelookupbyppage: null,
                LookupDiv: null,
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
                        //var rows = $('#' + tableName + ' tr:visible');
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
                        //var rows = $('#' + tableName + ' tr:visible');
                        var rows = document.getElementById(tableName).rows;
                        if (rows) {
                            var records = (rows.length - 1);
                            this.pages = Math.ceil(records / itemsPerPage);
                            this.inited = true;
                        }
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
                        pagerHtml += '<span onclick="' + pagerName + '.next();" class="pg-normal"><span style="cursor:pointer;">Next &#x21E8;</span></span>';

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
                                $(obj.appendToId).val(data[0]);
                                $(obj.appendTo).val(data[1]);

                                if (obj.appendToTextbox != null) {
                                    $("#nameofit").attr('data-id', data[0]);
                                }
                            }
                        }
                        else {
                            target.className = "";
                            if (obj.appendTo != null) {
                                $(obj.appendToId).val("");
                                $(obj.appendTo).val("");
                                if (obj.appendToTextbox != null) {
                                    // $("#nameofit").attr('data-id', '');
                                }
                            }
                        }
                    }
                }
            };
            var searchon = function () {
                $('#lookup-search').on('keyup', function (e) {
                    var key = e.which;
                    if (key == 13) {
                        var value = jQuery(this).val().toUpperCase().toLowerCase();
                        var $tablerows = $('#' + obj.setnameofthelookupbyppage + ' tr');
                        if (value.trim() === '') {
                            //$tablerows.show(500);
                            pageon();
                            $('#' + obj.pagename + '').show();
                            return false
                        }
                        $tablerows.each(function (index) {
                            if (index !== 0) {
                                $tablerows = $(this);
                                var column2 = $tablerows.find("td").eq(1).text().toUpperCase().toLowerCase();
                                if ((column2.indexOf(value) > -1)) {
                                    $tablerows.show();
                                }
                                else {
                                    $tablerows.hide();
                                }
                            }
                            //$('#' + obj.pagename + '').hide();
                        });
                        //pageon();
                    }
                });
            };
            var lookupajaxdata;
            var init = jQuery(this).dialog({
                autoOpen: false,
                width: 400,
                height: 500,
                modal: true,
                title: "Lookup Model",
                closeOnEscape: false,
                open: function (event, ui) {
                    $.CheckSessionExitance();
                    $(".ui-dialog-titlebar-close", ui.dialog | ui).on('click', function () {
                        jQuery(init).dialog().dialog('destroy').hide();
                    });
                    lookupajaxdata = $.ajax({
                        beforeSend: function () {
                            ajaxloader(init);
                            $(init).find("div .ajax_loder").show();
                        },
                        url: obj.lookupurl,
                        type: 'POST',
                        cache: false,
                        contentType: 'application/json',
                        datatype: 'json',
                    });
                    lookupajaxdata.done(function (successdata) {
                        $(init).find("div .ajax_loder").hide();
                        $(init).find("div .ajax_loder").remove();
                        var htmltag = '<div style=" margin: 0px auto; width: 181px; padding: 1px 0px 5px;"><label>Search : </label><input type="text" id="lookup-search" /></div><table class="lookuptable" id="' + obj.setnameofthelookupbyppage + '"><tr><th>Sr.No</th><th>Description</th></tr>'
                        jQuery.each(successdata, function (i, k) {
                            htmltag += '<tr tabindex="-1">' +
                                '<td>' + k.srno + '</td>' +
                                '<td>' + k.lookupvalue + '</td>' +
                                '</tr>'
                        });
                        htmltag += '</table>';
                        jQuery('.' + obj.LookupDiv + '').html(htmltag);
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

        $.fn.AddDataToTextboxNew = function (fn) {
            var obj = $.extend({
                appendTo: null,
                appendToId: null,
                appendToTextbox: null,
                lookupurl: null,
                NoOfRecordToShow: null,
                setnameofthelookupbyppage: null,
                LookupDiv: null,
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
                        pagerHtml += '<span onclick="' + pagerName + '.next();" class="pg-normal"><span style="cursor:pointer;">Next &#x21E8;</span></span>';

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


                                $(obj.appendToId).val($(obj.appendToId).val() + " " + data[0]);
                                $(obj.appendTo).val($(obj.appendTo).val() + " " + data[1]);

                                if (obj.appendToTextbox != null) {
                                    $("#nameofit").attr('data-id', data[0]);
                                }
                            }
                        }
                        else {
                            target.className = "";
                            if (obj.appendTo != null) {
                                $(obj.appendToId).val("");
                                $(obj.appendTo).val("");
                                if (obj.appendToTextbox != null) {
                                    // $("#nameofit").attr('data-id', '');
                                }
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
                    $.CheckSessionExitance();
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
                        jQuery('.' + obj.LookupDiv + '').html(htmltag);
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
                url: "Transcation/ByDefaultLoadEmp",
                success: function (data) {
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
                url: "Transcation/ByDefaultLoadEmp",
                success: function (data) {
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
        $.LoadLocationByDefault = function (single, filterid) {
            single = single || false;
            ReturnStructIds(filterid, function (data) {
                var forwarddata = JSON.stringify(data);
                $('#employee-table').find('td').remove();
                $('#employee-table').P2BTransactionTableDynamic('#employee-search', '#case-employee', 'Transcation/GetLocationList?geo_id=' + forwarddata + '', "", single);
            });

        };
        $.LoadEmpByDefaultV2 = function () {
            ReturnStructIdsV2(function (data) {
                $('#employee_table').find('td').remove();
                var forwarddata = JSON.stringify(data);
                $('#employee_table').P2BTransactionTableDynamic('#employee-search', '#case-employee', 'Transcation/Get_Emplist?geo_id=' + forwarddata + '', "");
            });
        };
        $.LoadEmpByDefaultV = function () {
            ReturnStructIdsV2(function (data) {
                $('#employee-table').find('td').remove();
                var forwarddata = JSON.stringify(data);
                $('#employee-table').P2BTransactionTableDynamic('#employee-search', '#case-employee', 'Transcation/Get_Emplist?geo_id=' + forwarddata + '', "");
            });
        };
        $.LoadCanByDefault = function (single, filterid) {
            single = single || false;
            ReturnStructIds(filterid, function (data) {
                var forwarddata = JSON.stringify(data);
                $('#employee-table').find('td').remove();
                $('#employee-table').P2BTransactionTableDynamic('#employee-search', '#case-employee', 'Transcation/Get_Candidatelist?geo_id=' + forwarddata + '', "", single);
            });

        };
        $.LoadEmpByDefaultWOGeoId = function (single, filterid) {
            single = single || false;
            $('#employee-table').find('td').remove();
            $('#employee-table').P2BTransactionTableDynamic('#employee-search', '#case-employee', 'Transcation/Get_EmployelistWOGeoId?Filter=' + filterid + '', "", single);

        };
        $.LoadEmpByDefaultPFTRUST = function (single, filterid) {
            single = single || false;
            ReturnStructIds(filterid, function (data) {
                var forwarddata = JSON.stringify(data);
                $('#employee-table').find('td').remove();
                $('#employee-table').P2BTransactionTableDynamic('#employee-search', '#case-employee', 'Transcation/Get_EmployelistPFT?geo_id=' + forwarddata + '', "", single);
            });

        };

        $.LoadEmpByDefaultLogin = function (single, filterid) {
            single = single || false;
            ReturnStructIds(filterid, function (data) {
                var forwarddata = JSON.stringify(data);
                $('#employee-table').find('td').remove();
                $('#employee-table').P2BTransactionTableDynamic('#employee-search', '#case-employee', 'Transcation/Get_EmployeeListLogin?geo_id=' + forwarddata + '', "", single);
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
            obj.btndisableid = '.popup-content-icon-edit,.popup-content-icon-remove,.popup-content-icon-view';
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
                    jQuery(init).find('select').empty().append("<option value=0 selected=true>-Select-</option>").selectmenu().selectmenu("refresh");
                    jQuery(init).find('input').empty();
                    jQuery(init).find('textarea').empty();
                    //jQuery(init).find(obj.btndisableid).button().button('enable').removeClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                    jQuery(init).find(obj.btndisableid).button().button('enable').removeClass('ButtonHover');
                    jQuery(init).find(obj.lookuptableid).find('tr td').parent().remove(); RemoveErrTag();
                },
                open: function (event, ui) {
                    $.CheckSessionExitance();
                    //$('.ui-dialog-titlebar-help').click(function () {
                    // helpfun("create", "" + obj.form.slice("4") + "");
                    //});
                    //createajaxdata = $.ajax({
                    // url: obj.creaturl,
                    // method: 'POST',
                    // data: { data: creadata }
                    //});
                    //createajaxdata.done(function (value) {

                    // returnfunctiondata(value);
                    //});
                    //jQuery(init).find(obj.btndisableid).button().button('disable').addClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                    jQuery(init).find(obj.btndisableid).button().button('disable').addClass('ButtonHover');
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
                                    $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('disable').addClass('submitbtndisable');
                                    ajaxloaderv2('body');

                                },
                            });
                            ajaxdata.done(function (msg) {
                                $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('enable').removeClass('submitbtndisable');
                                //$('.ajax_loder').parents('div').remove();
                                ajaxLoderRemove();
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
                                }

                                $(newDiv).dialog('open');
                                $('.ui-dialog-buttonpane').find('button:contains("Ok")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-circle-check"></span>');
                            });
                            ajaxdata.fail(function (msg) {
                                $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('enable').removeClass('submitbtndisable');
                                //$('.ajax_loder').parents('div').remove();
                                ajaxLoderRemove();
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
                submitfun: null,
                displaydialog: null
            }, fn);
            var init = $(this);
            obj.mode = obj.mode.toUpperCase();
            var del = "DELETE";
            var edit = "EDIT";
            var view = "VIEW";
            if (obj.mode != del) {
                var maindailog = jQuery(this).dialog({
                    autoOpen: false,
                    height: obj.height,
                    width: obj.width,
                    modal: true,
                    closeOnEscape: false,
                    title: obj.title,
                    beforeClose: function () {
                        jQuery(init).find("select").detach();
                        jQuery(init).remove(); RemoveErrTag();
                    },
                    open: function (event, ui) {
                        $.CheckSessionExitance();
                        function Appenddata() {
                            var editajaxopenloaddata = $.ajax({
                                url: obj.editurl,
                                method: 'POST',
                                data: { data: obj.editdata },
                            });
                            editajaxopenloaddata.done(function (value) {
                                if (typeof obj.returndatafunction === "function" && obj.mode != del) {
                                    obj.returndatafunction(value);
                                }
                                if (obj.mode.toUpperCase() == view) {
                                    $(init).find('input').attr('readonly', 'readonly');
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
                                //$(init).find("div .ajax_loder").hide();
                                ajaxLoderRemove(init);
                                $(init).html(result);
                                if (obj.mode != null && obj.mode != del && obj.editurl != null && obj.editdata != null) {
                                    Appenddata();
                                } else {
                                    obj.returndatafunction();

                                }
                            },
                            error: function (xhr, status, error) {
                                showajaxeroor(xhr, init);
                                $(init).find("div .ajax_loder").show();
                            },
                            complete: function () {
                                //$(init).find("div .ajax_loder").hide();
                                ajaxLoderRemove(init);
                            }
                        });
                    },
                    buttons: {
                        Submit: function () {
                            if (obj.mode.toUpperCase() == view) {
                                $(maindailog).dialog('close');
                                $(maindailog).remove();
                                if (typeof obj.submitfun === "function") {
                                    obj.submitfun('msg');
                                }
                            } else {
                                if (typeof obj.submiturl === "function") {
                                    obj.submiturl(function (data) {

                                        var newDiv = $(document.createElement('div'));
                                        var htmltag = '<p><span class="ui-icon ui-icon-check" style="float:left;margin-right:10px"></span> ' + data.responseText + '';
                                        htmltag += '</p>';
                                        newDiv.html(htmltag);
                                        newDiv.dialog({
                                            autoOpen: false,
                                            title: "Information",
                                            closeOnEscape: false,
                                            height: 150, width: 250, modal: true,
                                            buttons: {
                                                Ok: function () {
                                                    if (data.success == true) {
                                                        newDiv.dialog("close");
                                                        newDiv.remove();
                                                        jQuery(maindailog).dialog("close");
                                                        jQuery(init).remove();
                                                        if (typeof obj.submitfun === "function") {
                                                            obj.submitfun("");
                                                        }
                                                    } else {
                                                        newDiv.dialog("close");
                                                        newDiv.remove();
                                                        return false;
                                                    }
                                                }
                                            }
                                        });
                                        newDiv.dialog('open');
                                    });
                                }
                                else if (obj.submiturl == "") {
                                    //var chkdisplaydialog =
                                    obj.displaydialog();
                                    //chkdisplaydialog.done(function () {
                                    //    jQuery(maindailog).dialog("close");
                                    //    jQuery(init).remove();
                                    //});
                                } else {
                                    var ajaxdata = $.ajax({
                                        url: obj.submiturl,
                                        method: "POST",
                                        beforeSend: function () {
                                            $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('disable').addClass('submitbtndisable');
                                            ajaxloaderv2('body');
                                        },
                                        data: $(obj.form).serialize() + '&data=' + obj.forwarddata + '',
                                    });
                                    ajaxdata.done(function (msg) {
                                        $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('enable').removeClass('submitbtndisable');
                                        //$('.ajax_loder').parents('div').remove();
                                        ajaxLoderRemove();

                                        if (msg.status == true) {

                                            var newDiv = $(document.createElement('div'));
                                            var htmltag = '<p><span class="fa fa-fw fa-3x fa-check-circle-o ajax-success-icon" style="float:left;margin-right:10px"></span> ' + msg.responseText + '';
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
                                                        jQuery(maindailog).dialog("close");
                                                        jQuery(init).remove();
                                                        if (typeof obj.submitfun === "function") {
                                                            obj.submitfun(msg);
                                                        }
                                                    }
                                                }
                                            });
                                            newDiv.dialog('open');

                                        } else {
                                            var newDiv = $(document.createElement('div'));
                                            var htmltag = '<p><span class="fa fa-fw fa-3x fa-exclamation-circle ajax-error-icon" style="float:left;margin-right:10px"></span> ' + msg.responseText + '';
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
                                                        // jQuery(maindailog).dialog("close");
                                                        // jQuery(init).remove();
                                                        if (typeof obj.submitfun === "function") {
                                                            obj.submitfun(msg);
                                                        }
                                                    }
                                                }
                                            });
                                            newDiv.dialog('open');

                                        }
                                    });
                                    ajaxdata.fail(function (jqXHR, textStatus) {
                                        $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('enable').removeClass('submitbtndisable');
                                        //$('.ajax_loder').parents('div').remove();
                                        ajaxLoderRemove();
                                        var newDiv = $(document.createElement('div'));
                                        var htmltag = '<p><span class="ui-icon ui-icon-alert" style="float:left;margin-right:10px"></span>' + jqXHR.status + '"-"' + jqXHR.statusText + '';
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

                        }
                    },
                    Cancel: function () {
                        jQuery(init).dialog("close");
                    }

                });
                jQuery(maindailog).dialog(obj.state);
            } else {
                var deleteajaxdata,
                    deletedata, forwarddata = 0;
                var newDiv = $(document.createElement('div'));
                var htmltag = '<p><span class="ui-icon ui-icon-alert" style="float:left;margin-right:10px"></span> ' + "Are You Sure You Want To Delete Record..?" + '';
                htmltag += '</p>';
                newDiv.html(htmltag);
                newDiv.dialog({
                    autoOpen: false,
                    title: "Delete Confirmation !",
                    height: obj.height,
                    width: obj.width,
                    modal: true,
                    beforeClose: function () {
                        newDiv.remove(); RemoveErrTag();
                    },
                    buttons: {
                        Confirm: function () {
                            if (typeof obj.submitfun === "function") {
                                if (obj.editdata != null) {

                                    var ajx = $.ajax({
                                        url: obj.submiturl,
                                        method: "POST",
                                        data: { data: obj.forwarddata.toString(), data2: obj.editdata.toString() },
                                        success: function (data) {
                                            // if (data.status == true) {
                                            data.valid = data.valid || true;
                                            var newDivChild = $(document.createElement('div'));
                                            var htmltag = '<p><span class="ui-icon ui-icon-alert" style="float:left;margin-right:10px"></span> ' + data.responseText + '';
                                            htmltag += '</p>';
                                            newDivChild.html(htmltag);
                                            newDivChild.dialog({
                                                autoOpen: false,
                                                title: "Information", closeOnEscape: false,
                                                height: 130, width: 250, modal: true,
                                                buttons: {
                                                    Ok: function () {
                                                        newDivChild.dialog("close");
                                                        if (data.valid == true) {
                                                            jQuery(newDiv).dialog("close");
                                                            obj.submitfun("Record Remove");
                                                        }
                                                    }
                                                }
                                            });
                                            newDivChild.dialog('open');
                                            //
                                        }
                                    });
                                }
                                else {

                                    var ajx = $.ajax({
                                        url: obj.submiturl,
                                        method: "POST",
                                        data: { data: obj.forwarddata.toString() },
                                        success: function (data) {
                                            // if (data.status == true) {
                                            data.valid = data.valid || true;
                                            var newDivChild = $(document.createElement('div'));
                                            var htmltag = '<p><span class="ui-icon ui-icon-alert" style="float:left;margin-right:10px"></span> ' + data.responseText + '';
                                            htmltag += '</p>';
                                            newDivChild.html(htmltag);
                                            newDivChild.dialog({
                                                autoOpen: false,
                                                title: "Information", closeOnEscape: false,
                                                height: 130, width: 250, modal: true,
                                                buttons: {
                                                    Ok: function () {
                                                        newDivChild.dialog("close");
                                                        if (data.valid == true) {
                                                            jQuery(newDiv).dialog("close");
                                                            obj.submitfun("Record Remove");
                                                        }
                                                    }
                                                }
                                            });
                                            newDivChild.dialog('open');
                                            //
                                        }
                                    });
                                }
                            }
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
                formname.find('select').empty().append('<option></option>').selectmenu().selectmenu("refresh");
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
            formname.find('select').empty().append('<option></option>').selectmenu().selectmenu("refresh");
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
        $.fn.FormatDate = function (d) {
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
                $('<div></div>').P2BMessageModalDialog('ui-icon-alert', "Please Select Row..!");
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
                    jQuery(init).remove(); RemoveErrTag();
                },
                open: function (event, ui) {
                    $.CheckSessionExitance();
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
                            //$(init).find('button').buttonset("option", "disabled", true);
                            $(init).find('button').prop("disabled", true);


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
                            //$(init).find("div .ajax_loder").hide();
                            ajaxLoderRemove();
                            $(init).html(result);
                            Appenddata();
                        },
                        error: function (xhr, status, error) {
                            showajaxeroor(xhr, init);
                            $(init).find("div .ajax_loder").show();
                        },
                        complete: function () {
                            //$(init).find("div .ajax_loder").hide();
                            ajaxLoderRemove(init);
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
                jQuery(filter_drop).empty().append("<option value=0 selected=true>-Select-</option>").selectmenu().selectmenu("refresh");
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
                        jQuery(filter_drop).selectmenu().selectmenu("refresh").selectmenu("menuWidget").css({ "height": "auto" });
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
            jQuery(init).empty().append(htm).selectmenu().selectmenu("refresh");
            $.post(url, { data2: forwardata, data3: forwardata2 }, function (data) {
                $.each(data, function (i, k) {
                    jQuery(init).append($('<option>', {
                        value: k.Value,
                        text: k.Text,
                        selected: k.Selected
                    }));
                });
                jQuery(init).selectmenu().selectmenu("refresh").selectmenu("menuWidget").css("height", "100px");
            });
            jQuery(drop2).empty().append(htm).selectmenu().selectmenu("refresh");
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
                    if (e.which == 13) {
                        var $rows = $('#' + init.attr('id') + ' tr');
                        var value = jQuery(this).val().toUpperCase().toLowerCase();
                        if (value == '') {
                            $rows.removeClass('table-div-hide');
                            return false;
                        }
                        $rows.each(function (index) {
                            if (index !== 0) {
                                var $row = $(this);
                                var column2 = $row.find("td").eq(2).text().toUpperCase().toLowerCase();
                                if ((column2.indexOf
                                    (value) > -1)) {
                                    $row.removeClass('table-div-hide');
                                }
                                else {
                                    $row.addClass('table-div-hide');
                                }
                            }
                        });
                    } else if (e.keyCode == 8) {
                        var $rows = $('#' + init.attr('id') + ' tr');
                        $rows.removeClass('table-div-hide');
                    }
                });
            };
            var RemoveElement = function () {
                $(init).on("click", "tr td span.transcation_delete", function (e) {
                    $(this).parents('tr').remove();
                });
            }
            function checkboselection() {

                $(IDofCheckbox).off('click').on('click', function (e) {
                    var b = init.attr('id');

                    if (this.checked) {
                        $('#' + b + ' .case').each(function (i, k) {
                            var element = $(k);
                            var parent = element.parent('td').parent('tr');
                            if (!parent.hasClass('table-div-hide')) {
                                if (!element.is('checked')) {
                                    element.attr('checked', 'checked');
                                    element.prop('checked', true);
                                    parent.addClass('selectedtr');
                                }
                            }
                        });
                    } else {

                        $('#' + b + ' .case').each(function (i, k) {
                            var ele = $(k);
                            var parent = ele.parent('td').parent('tr');
                            if (!parent.hasClass('table-div-hide')) {
                                parent.removeClass('selectedtr');
                                ele.removeAttr('checked');
                            }

                        });
                    }

                });

                $(document).on('click', '#' + init.attr('id') + ' .case', function (e) {
                    // alert('hi')
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
                $(init).find('tr:gt(0)').remove();
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
        $.DateFormateStandardToDDMMYY = function (d) {
            var dd = parseInt(d.getDate());
            var mm = parseInt(d.getMonth() + 1);
            var yy = parseInt(d.getFullYear());
            if (dd < 10) {
                dd = '0' + dd;
            } if (mm < 10) {
                mm = '0' + mm;
            }
            return dd + "/" + mm + "/" + yy;
        };
        $.DateFormateDDMMYYToStandard = function (date) {
            var temp = date.split("/");
            return new Date(parseInt(temp[2]), parseInt(temp[1]) - 1, parseInt(temp[0]));
        };
        $.DateCheckWithTodayDate = function (date, oper) {
            if (oper == "<") {
                var dateval = $(date).val();
                if (new Date() < $.DateFormateDDMMYYToStandard(dateval)) {
                    return 0;
                }
            } else if (oper == ">") {
                var dateval = $(date).val();
                if (new Date() > $.DateFormateDDMMYYToStandard(dateval)) {
                    return 0;
                }
            }
        };
        $.StartEndDateCheck = function (fn) {
            var Obj = $.extend({
                startdate: null,
                enddate: null
            }, fn);
            if ($.DateFormateDDMMYYToStandard($(Obj.startdate).val()) > $.DateFormateDDMMYYToStandard($(Obj.enddate).val())) {
                return 0;
            }
        };
        $.fn.AppendCheckbox = function (fn) {
            var obj = $.extend({
                dataurl: null,
            }, fn);
            var $this = $(this);
            $.ajax({
                url: obj.dataurl,
                success: function (data) {
                    if (data.success == true) {
                        var htm = "";
                        $.each(data.data, function (i, k) {
                            htm += "<li class=checkboxclass ><input type=checkbox class=remarkclass name=" + k.Id + " id=" + k.data + "_div" + " /> <label for=" + k.data + "_div" + ">" + k.name + "</label></li>";
                        });
                        $this.append(htm);
                    }
                }
            });
        };
        $.getUrlVars = function () {
            var vars = [], hash;
            var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
            for (var i = 0; i < hashes.length; i++) {
                hash = hashes[i].split('=');
                vars.push(hash[0]);
                vars[hash[0]] = hash[1];
            }
            return vars;
        };

        var PayMonthObj = {
            Month: null,
            SetMonth: function (val) {
                if (val != null) {
                    this.Month = val
                } else {
                    this.Month = null;

                }
            },
            GetMonth: function () {
                return this.Month;
            }

        };
        $.fn.MultiLevelInlineEditGrid = function (fn) {

            var obj = $.extend({
                url: null,
                columnname: null,
                editurl: null,
                htmurl: null,
                submiturl: null,
                childheader: null,
                childurl: null,
                childurlColumnNo: null,
                tableheader: null
            }, fn);
            var column = [
                {
                    "class": "details-control",
                    "orderable": false,
                    "defaultContent": "",
                    "bSearchable": false,
                    "bSortable": false,
                }
            ];
            for (var i = 0; i < obj.columnname.length; i++) {
                column.push({ "data": obj.columnname[i] });
            }

            var selected = [],
                init = $(this),
                detailRows = [];
            var searchdata = "";
            var dt = $(init).DataTable({
                jQueryUI: true,
                bServerSide: true,
                ajax: {
                    "url": obj.url,
                    "type": "POST",
                    "data": function (d) {
                        d.sSearch = $('.dataTables_filter input').val()
                    },
                    "dataSrc": function (data) {
                        if (data.data != null && data.data != undefined) {
                            return data.data;
                        } else {
                            return null;
                        }
                    }
                },
                initComplete: function () {
                    $('.dataTables_filter input').off();
                    $('.dataTables_filter input').on('keyup', function (e) {
                        if (e.keyCode == 13) {
                            dt.draw();
                        } else {
                            e.preventDefault();
                        }
                    });
                },
                scrollCollapse: false,
                paging: false,
                scrollY: '30vh',
                bProcessing: true,
                responsive: true,
                columns: column,
                order: [[1, 'asc']],
                destroy: true,
            });
            $('div.ui-corner-tr').append("<div class='datatable-tableheader-class'>" + obj.tableheader + "</div>");


            $('#' + $(init).attr('id') + ' tbody').on('click', 'tr', function () {
                if ($(this).hasClass('selected')) {
                    $(this).removeClass('selected');
                    localStorage.setItem($(init).attr('id') + '_selected', "");
                }
                else {
                    var aa = JSON.stringify($('#' + $(init).attr('id') + '').DataTable().row(this).data());

                    localStorage.setItem($(init).attr('id') + '_selected', aa);
                    dt.$('tr.selected').removeClass('selected');
                    $(this).addClass('selected');
                }
            });
            $('#' + $(init).attr('id') + ' tbody').on('click', 'td.details-control', function () {
                var tPayMonthObj = $.LocalStorageHelper("LoanAdvRequest_LoadEmp");
                //$.LocalStorageHelper("LoanAdvRequest_LoadEmp");
                //
                var tr = $(this).parents('tr');
                var row = dt.row(tr);
                var idx = "";
                if (obj.childurlColumnNo != null) {
                    idx = $(tr.find("td:eq(" + obj.childurlColumnNo + ")")).text();
                } else {
                    idx = $(tr.find("td:eq(1)")).text();
                }
                if (row.child.isShown()) {
                    tr.removeClass('details');
                    row.child.hide();
                }
                else {
                    tr.addClass('details');
                    $.ajax({
                        url: obj.childurl,
                        type: 'POST',
                        datatype: 'json',
                        data: { data: idx, filter: tPayMonthObj },
                        success: function (res) {
                            var coldata;
                            var td = "";
                            var value = '<table class="table" id="table"><tr><th>DD</th>'
                            $.each(obj.childheader, function (i, k) {
                                value += '<th>' + k + '</th>'
                            });
                            value += '</tr>';
                            function AddChild(k) {
                                var temphtml = "";
                                $.each(obj.childheader, function (i, l) {
                                    temphtml += "<td>" + k[l] + "</td>";
                                });
                                return temphtml;
                            };
                            var Loadbuttonsforitinvestment = obj.childurl;
                            var Loadbuttonsforitinvestmentchk = Loadbuttonsforitinvestment.includes("/ITInvestmentPayment80C/Get_ITinvestmentPaymentDetails");
                            var Loadbuttonsforitinvestmentchk1 = Loadbuttonsforitinvestment.includes("/ITInvestmentPayment80CCCto80CCF/Get_ITinvestmentPaymentDetails");
                            var Loadbuttonsforitinvestmentchk2 = Loadbuttonsforitinvestment.includes("/ITInvestmentPaymentSection80Dto80U/Get_ITinvestmentPaymentDetails");
                            var Loadbuttonsforitinvestmentchk3 = Loadbuttonsforitinvestment.includes("/ITSection24Payment/Get_ITSection24Payment");
                            var Loadbuttonsforitinvestmentchk4 = Loadbuttonsforitinvestment.includes("/LvNewReq/Get_LvNewReq");
                            if (Loadbuttonsforitinvestmentchk == true || Loadbuttonsforitinvestmentchk1 == true || Loadbuttonsforitinvestmentchk2 == true || Loadbuttonsforitinvestmentchk3 || Loadbuttonsforitinvestmentchk4 == true) {

                                $.each(res, function (i, k) {
                                    value += '<tr>' +
                                        '<td><ul class="inline-btn">' +
                                        '<li class="ui-icon ui-icon-newwin" onclick="$.DataTableChildManipulation(this,' + k.Id + ',' + "'" + idx + "'" + ',' + "'" + obj.htmurl + "'" + ',' + "'" + obj.editurl + "'" + ',' + "'" + obj.submiturl + "'" + ')" title="View Record"></li>' +
                                        '<li class="ui-icon ui-icon-pencil" onclick="$.DataTableChildManipulation(this,' + k.Id + ',' + "'" + idx + "'" + ',' + "'" + obj.htmurl + "'" + ',' + "'" + obj.editurl + "'" + ',' + "'" + obj.submiturl + "'" + ')" title="Edit Record"></li>' +
                                        '<li class="ui-icon ui-icon-trash" onclick="$.DataTableChildManipulation(this,' + k.Id + ',' + "'" + idx + "'" + ',' + "'" + obj.submiturl + "'" + ')" title="Delete Record"></li>' +
                                        '<li class="ui-icon fa fa-upload" onclick="$.DataTableChildManipulation(this,' + k.Id + ',' + "'" + idx + "'" + ',' + "'" + obj.editurl + "'" + ',' + "'" + obj.editurl + "'" + ',' + "'" + obj.submiturl + "'" + ')" title="Upload Image"></li>' +
                                        '<li class="ui-icon ui-icon-image" onclick="$.DataTableChildManipulation(this,' + k.Id + ',' + "'" + idx + "'" + ',' + "'" + obj.submiturl + "'" + ',' + "'" + obj.editurl + "'" + ',' + "'" + obj.submiturl + "'" + ')" title="Image Viewer"></li>' +

                                        // '<li class="ui-icon ui-icon-trash" onclick="$.DataTableChildManipulation(this,' + k.Id + ',' + "'" + obj.submiturl + "'" + ')" title="Delete Record"></li>' +
                                        '</ul></td>' + AddChild(k) + '</tr>';
                                });

                            } else {
                                $.each(res, function (i, k) {
                                    value += '<tr>' +
                                        '<td><ul class="inline-btn">' +
                                        '<li class="ui-icon ui-icon-newwin" onclick="$.DataTableChildManipulation(this,' + k.Id + ',' + "'" + idx + "'" + ',' + "'" + obj.htmurl + "'" + ',' + "'" + obj.editurl + "'" + ',' + "'" + obj.submiturl + "'" + ')" title="View Record"></li>' +
                                        '<li class="ui-icon ui-icon-pencil" onclick="$.DataTableChildManipulation(this,' + k.Id + ',' + "'" + idx + "'" + ',' + "'" + obj.htmurl + "'" + ',' + "'" + obj.editurl + "'" + ',' + "'" + obj.submiturl + "'" + ')" title="Edit Record"></li>' +
                                        '<li class="ui-icon ui-icon-trash" onclick="$.DataTableChildManipulation(this,' + k.Id + ',' + "'" + idx + "'" + ',' + "'" + obj.submiturl + "'" + ')" title="Delete Record"></li>' +

                                        // '<li class="ui-icon ui-icon-trash" onclick="$.DataTableChildManipulation(this,' + k.Id + ',' + "'" + obj.submiturl + "'" + ')" title="Delete Record"></li>' +
                                        '</ul></td>' + AddChild(k) + '</tr>';
                                });
                            }
                            value += "</table>";
                            row.child(value).show();
                        }
                    });
                }
            });
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
                if (obj.setTimeout == true)
                    setTimeout(function () { $('#snackbar').removeClass('show'); }, 3000);
            }
        };
        $.removeDisble = function (frm) {
            var all_element = $('form' + frm + ' :input').toArray();
            $.each(all_element, function (i, k) {
                if ($(k)[0].nodeName == "INPUT") {
                    $(k).removeAttr('readonly');
                    $(k).removeAttr('disabled');

                }
            });
        };
        $.fn.P2BConfidentialModelDialog = function (fn) {
            var obj = $.extend({
                passurl: null,
                htmlurl: null,
                submiturl: null,
                BeforeSendurl: null,
                form: null,
                height: null,
                width: null,
                title: null,
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
                    jQuery(init).remove(); RemoveErrTag();
                },
                open: function (event, ui) {
                    $.CheckSessionExitance();
                    jQuery.ajax({
                        url: obj.htmlurl,
                        method: "GET",
                        beforeSend: function () {
                            if (obj.BeforeSendurl != null) {
                                $.ajax({
                                    url: obj.BeforeSendurl,
                                    method: "POST",
                                    success: function (data) {
                                        if (data.success == true) {
                                            alert("The File is Allready Exists!Are You Want to replace It")
                                        }
                                    }
                                });
                            }
                            ajaxloader(init);
                            $(init).find("div .ajax_loder").show();
                        },
                        success: function (result, status, xhr) {
                            showajaxeroor(xhr, init);
                            $(init).find("div .ajax_loder").hide();
                            $(init).html(result);
                            if (obj.title != "DocumentUpload") {
                                ValidateUser(init, obj.passurl);
                            }
                            //OnEnterFocusNext();
                            AlterBtnType();
                            // $(classoridoftheonwhichpopupderived).find(nameidclassofbuttontodisable).button('disable').button().button("refresh").addClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
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
                        var a = $(obj.form).doval();
                        if (x == true && a == true) {
                            if (obj.type == null) {
                                ajaxdata = $.ajax({
                                    url: obj.submiturl,
                                    method: "POST",
                                    data: $(obj.form).serialize() + '&forwarddata=' + obj.forwarddata + '',
                                    beforeSend: function () {
                                        // alert('hiihihihihi');
                                        $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('disable').addClass('submitbtndisable');
                                        ajaxloaderv2(init);

                                    },
                                });
                                ajaxdata.done(function (msg) {
                                    $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('enable').removeClass('submitbtndisable');
                                    $('.ajax_loder').parents('div').remove();
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
                                    $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('enable').removeClass('submitbtndisable');
                                    $('.ajax_loder').parents('div').remove();
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
                            } else if (obj.type.toUpperCase() == "FILE") {
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
                                        $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('disable').addClass('submitbtndisable');
                                        ajaxloaderv2('body');

                                    },
                                });
                                ajaxdata.done(function (msg) {
                                    $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('enable').removeClass('submitbtndisable');
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
                                                    obj.returnfun(msg);
                                                    // jQuery(dia).dialog("close");
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
                                ajaxdata.fail(function (xhr, status, error) {
                                    $('.ui-dialog-buttonpane').find('button:contains("Submit")').button().button('enable').removeClass('submitbtndisable');
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
        $.fn.P2BSelectMenuMuliSelectAppend1 = function (url, forwardata, forwardata2) {
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
        $.fn.ProcessProgressFn = function (fn) {
            var obj = $.extend({
                passurl: null,
                htmlurl: null,
                submiturl: null,
                form: null,
                height: null,
                width: null,
                title: null,
                returnfun: null,
                forwarddata: null
            }, fn);
        };
        $.OnlyOneYesAllowed = function (fn) {
            var init = fn.split(",");
            $.each(init, function (i, k) {
                $("[name='" + k + "']").on('change', function (e) {
                    var current_target = e.target.name;
                    if ($(this).val() == 'true') {
                        $('.' + current_target + '-class').show();
                    } else {
                        $('.' + current_target + '-class').hide();
                    }
                    var NotThisoOne = $(init).filter(function (i, k) {
                        return k != current_target;
                    });
                    $.each(NotThisoOne, function (i, k) {
                        $("[name='" + k + "']").val([false]).button().button("refresh");
                        $('.' + k + '-class').hide();
                    });
                });
            });

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

        $.fn.ChartHelper = function (fn) {
            /*
            Dependancy chartjs
            */
            var obj = $.extend({
                url: null,
                type: null,
                title: null,
            }, fn);
            var init = $(this);
            var id = "#" + $(this).attr('id');
            $.ajax({
                url: obj.url,
                success: function (data) {
                    var horizontalBarChartData = JSON.parse(data);
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
                                display: true,
                                text: obj.title
                            },
                        }
                    });
                }
            });
        };
        $.fn.P2BAuthorizeModalDialog = function (openurl, opendataforward, editurl, maindialogtitle, forwardserializedata, forwarddata, editmessage, editerrormessage, gridreloadname, height, width, nameofthelookuptable, nameidclassofbuttontodisable, returndatafunction, fn) {
            var originalObjectData;
            var DTObjectData;
            var editajaxdata, editajaxopenloaddata, init;
            var OldIds = [];
            var NewIds = [];
            var olddata = [];
            //nameidclassofbuttontodisable = '.popup-content-icon-edit,.popup-content-icon-remove,.popup-content-icon-view';
            nameidclassofbuttontodisable = ".popup-content-icon-create,.popup-content-icon-edit,.popup-content-icon-remove,.popup-content-icon-lookup,.popup-content-icon-view";
            var confrm, flag;
            init = jQuery(this);            
            var maindialog = jQuery(init).dialog({
                autoOpen: false,
                height: height,
                width: width,
                modal: true,
                closeOnEscape: false,
                title: maindialogtitle,
                beforeClose: function (e) {
                    RemoveLookupTableElement(forwardserializedata); RemoveErrTag();

                    
                },
                open: function (event, ui) {
                    $.CheckSessionExitance();
                    $('.ui-dialog-titlebar-help').html('<span class="ui-button-icon ui-icon ui-icon-help"></span>');
                    NewIds = [];
                    olddata = [];
                    OldIds = [];
                    var allDataForAPI = JSON.stringify(P2bBindAllDataForEdit(opendataforward));
                    editajaxopenloaddata = $.ajax({
                        url: openurl,
                        method: 'POST',
                        contentType: 'application/json',
                        data: allDataForAPI,
                        //data: { data: opendataforward },
                        beforeSend: function () {
                            ajaxloaderv2('body');
                        },
                        complete: function () {
                            // $('.ajax_loder').parents('div').remove();
                            ajaxLoderRemove();
                        }
                    });
                    editajaxopenloaddata.done(function (value) {
                        if (typeof returndatafunction === 'function') {
                            if (value && value.Data) {
                                if (!singleData['AutoAuthorization'] && !singleData['Action'].includes('C')) {
                                    DTObjectData = value.Data.DTData;
                                    originalObjectData = value.Data.OriginalData;
                                } else {
                                    DTObjectData = value.Data.OriginalData;
                                }
                            }
                            returndatafunction(value);
                        } else {
                            console.log('returnfun is not define');
                        }
                        if (DTObjectData && DTObjectData['Action'] !== 'E') {
                            $('.ui-dialog-buttonpane').find('button:contains("Compare")').button('disable');
                        }
                    });
                    OnpageAlter();
                    jQuery(this).find("select").selectmenu('disable').addClass("ui-selectmenu-text");
                    jQuery(this).find('input').prop('disabled', true).css("background-color", "rgba(241, 241, 241, 0.66)");
                    jQuery(this).find('textarea').prop('disabled', true).css("background-color", "rgba(241, 241, 241, 0.66)");
                    //jQuery(init).find(nameidclassofbuttontodisable).button().button('disable').addClass('ButtonHover').css("background-color", "rgba(241, 241, 241, 0.66)");
                    jQuery(init).find(nameidclassofbuttontodisable).button().button('disable').addClass('ButtonHover');
                },
                buttons: {
                    Approve: function (e) {
                        ApproveRejectBtnClickFunc(forwardserializedata, init, maindialog, 'Approve', 1, forwarddata, editurl, gridreloadname, DTObjectData);
                    },
                    Reject: function () {
                        ApproveRejectBtnClickFunc(forwardserializedata, init, maindialog, 'Reject', 2, forwarddata, editurl, gridreloadname, DTObjectData);
                    },
                    Compare: function () {
                        if (DTObjectData['Action'] === 'E') {
                            P2bCreateAuthorizationDialog({ FormName: forwardserializedata, OriginalData: originalObjectData, ModifiedData: DTObjectData, Forwarddata: forwarddata });
                        }
                    },
                    Cancel: function () {
                        jQuery(init).dialog('close');
                    }
                }
            });
            jQuery(init).dialog('open');
            var submitIcon = $('.ui-icon-check');
            if (submitIcon.length === 0) {
                submitIcon = '<span class="ui-icon ui-icon-check"></span>';
            }
            var rejectIcon = $('.ui-icon-check');
            if (rejectIcon.length === 0) {
                rejectIcon = '<span class="ui-icon ui-icon-cancel"></span>';
            }
            var compareIcon = $('.ui-icon-check');
            if (compareIcon.length === 0) {
                compareIcon = '<span class="ui-icon ui-icon-transferthick-e-w"></span>';
            }
            var cancelIcon = $('.ui-icon-check');
            if (cancelIcon.length === 0) {
                cancelIcon = '<span class="ui-icon ui-icon-closethick"></span>';
            }
            $('.ui-dialog-buttonpane').find('button:contains("Approve")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend(submitIcon);
            $('.ui-dialog-buttonpane').find('button:contains("Reject")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend(rejectIcon);
            $('.ui-dialog-buttonpane').find('button:contains("Compare")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend(compareIcon);
            //$('.ui-dialog-buttonpane').find('button:contains("Submit")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend('<span class="ui-icon ui-icon-disk"></span>');
            $('.ui-dialog-buttonpane').find('button:contains("Cancel")').removeClass('ui-button-text-only').addClass('ui-button-text-icon-primary').prepend(cancelIcon);
        };
    }));

class P2bFormDataHandlingClass {
    constructor() {
        this.data = [];
    }

    addData(...args) {
        this.data = [...this.data, ...args];
    }

    getData() {
        return this.data;
    }

    P2bCreateListOfTableData(formData, allTableIds) {
        const arrayOfTableName = [];
        if (allTableIds && typeof allTableIds === 'string') {
            const arrayOfTableIds = allTableIds.split(',');
            arrayOfTableIds.map(id => {
                const tableName = id.replace(/#/g, '');
                arrayOfTableName.push(tableName);
            })

            if (arrayOfTableName.length > 0) {
                arrayOfTableName.map(name => {
                    if (Array.isArray(formData[name]) && !name.includes("_Id")) {
                        formData[name] = formData[name].map((value, index) => (
                            { Id: value }
                        ))
                    } else if (!name.includes("_Id")) {
                        formData[name] = [{ Id: formData[name] }];
                    }
                    else {
                        if (Array.isArray(formData[name])) {
                            formData[name] = formData[name][0];
                        } else {
                            formData[name] = formData[name];
                        }
                    }
                })
            }
        }
        return formData;
    }

    P2bCreateListOfTableDataWithAction(formData, allTableIds) {
        const arrayOfTableName = [];
        const listOfTableObject = {};
        if (allTableIds && typeof allTableIds === 'string') {
            const arrayOfTableIds = allTableIds.split(',');
            arrayOfTableIds.map(id => {
                const tableName = id.replace(/#/g, '');
                arrayOfTableName.push(tableName);
            })
            if (arrayOfTableName.length > 0) {
                arrayOfTableName.map(name => {
                    var objData = document.getElementsByClassName(name);
                    if (Array.isArray(formData[name]) && !name.includes("_Id")) {
                        listOfTableObject[name] = formData[name].map((value, index) => (
                            { Id: value, Action: objData[index] && objData[index].innerText }

                        ))
                    } else if (!name.includes("_Id")) {
                        listOfTableObject[name] = [{ Id: formData[name], Action: objData[0] && objData[0].innerText }];
                    } else {
                        if (Array.isArray(formData[name])) {
                            listOfTableObject[name] = formData[name].map((value, index) => (
                                { Id: value, Action: objData[index] && objData[index].innerText }
                            ))
                            //listOfTableObject[name] = [{ Id: formData[name], Action: objData[0] && objData[0].innerText }];
                            //formData[name] = formData[name][0];
                        } else {
                            if (formData[name]) {
                                listOfTableObject[name] = [{ Id: formData[name], Action: objData[0] && objData[0].innerText }];
                            }
                        }
                    }
                })
            }
        }
        return listOfTableObject;
    }
}

function P2bGetFullDetails(obj) {
    function P2bCreateFullDetails(singleObject) {
        var getFullDetailsAsString = '';
        const allValuesFromObject = Object.values(singleObject);
        if (typeof allValuesFromObject === 'object') {
            const filterValues = allValuesFromObject.filter((item) => {
                if (item !== null || item !== "" || item !== undefined || typeof item !== "boolean") {
                    return item;
                }
            });

            filterValues.map((item) => {
                getFullDetailsAsString += item + ", ";
            })
        }
        return getFullDetailsAsString;
    }
    if (obj && typeof obj === 'object') {
        if (Array.isArray(obj)) {
            let fullDetailsArray = [];
            obj.map(obj => {
                var fullDetailsString = P2bCreateFullDetails(obj);
                fullDetailsArray.push(fullDetailsString);
            });
            return fullDetailsArray;
        } else {
            var fullDetailsString = P2bCreateFullDetails(obj);
            return fullDetailsString;
        }

    }
}
function DateConvert(dateString) {
    var date = new Date(dateString);
    //var date = dateString.substr(0, 10);
    //const splitDateArray = date.split('-');
    //date = splitDateArray[0] + "-" + splitDateArray[1] + "-" + splitDateArray[2];
    //date = new Date(date);
    //var displayDate = $.datepicker.formatDate("dd/mm/yy", date);
    var displayDate = $.datepicker.formatDate("yy-mm-dd", date);
    return displayDate;
}

function CreateDisableButtonsString(activeButtonsArray) {
    var disabledButtonsIds = [];
    var disableButtonIdString = '';
    if (!activeButtonsArray.includes('A')) {
        disabledButtonsIds.push('#Autho')
    }
    if (!activeButtonsArray.includes('C')) {
        disabledButtonsIds.push('#Create')
    }
    if (!activeButtonsArray.includes('D')) {
        disabledButtonsIds.push('#Delete')
    }
    if (!activeButtonsArray.includes('E')) {
        disabledButtonsIds.push('#Edit')
    }
    if (!activeButtonsArray.includes('L')) {
        disabledButtonsIds.push('#Lock')
    }
    if (!activeButtonsArray.includes('P')) {
        disabledButtonsIds.push('#Process')
    }
    if (!activeButtonsArray.includes('R')) {
        disabledButtonsIds.push('#Release')
    }
    if (!activeButtonsArray.includes('V')) {
        disabledButtonsIds.push('#View')
    }
    if (disabledButtonsIds.length > 0) {
        disableButtonIdString = disabledButtonsIds.join(',');
    }

    return disableButtonIdString;
}
//const APIURL = "http://192.168.1.11/P2B.API_2.0";
const APIURL = "http://192.168.1.12/P2B.API_2.0";
//const UserGroup = "ADMIN";
let lookUpActions = {};
let Action;
let NavListAction;
const IsAuthorisedObject = { Yes: true, No: false };
//let UserName = '00001886'; //Checker
 //let UserName = '00000357'; //Maker
 let UserName = '00000228'; //Admin
let AuthorizeStatus = IsAuthorisedObject['No'];
const User = [
    {
        EmpCode: '00001886', GroupId: 3, Action: ['A', 'L', 'R', 'V'], AutoAuthorization: false  //Checker
    },
    {
        EmpCode: '00000357', GroupId: 2, Action: ['A', 'C', 'D', 'E', 'P', 'V'], AutoAuthorization: false  //Maker
    },
    {
        EmpCode: '00000228', GroupId: 1, Action: ['C', 'E', 'V', 'D', 'P', 'A', 'R', 'L'], AutoAuthorization: true  //Admin
    }
];
var singleData = User.find((e) => e.EmpCode === UserName);