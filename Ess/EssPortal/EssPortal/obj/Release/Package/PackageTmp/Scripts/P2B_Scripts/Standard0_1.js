$(function () {
    "use strict"
    $.fn.r_CreateDialog = function (fn) {
        var Obj = $.extend({
            init: '#' + $(this).attr('id') + '',
            dataurl: null,
            htmurl: null,
            editurl: null,
            title: null,
            form: null,
            submiturl: null,
            mode: null,
            returndatafunction: null,
            submitfun: null,

        }, fn);
        var id, $this = $(this);
        var init = function (data) {
            /*
            *   Initialise -Store option on element
            */
            //   console.log(data);
            $this.data('r_CreateDialog', data);
        };
        var getData = function () {
            /*
                Get Data On Element
            */
            return $this.data('r_CreateDialog');
        };
        if (fn) {
            init(Obj);
        };
        function create() {

        };
        function edit() {

        }
        return {
            Create: create,
            Edit: edit,
            View: main,
            Delete: main,
        }
    };
    var a = {
        b: null,
    };
    
});