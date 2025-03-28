(function ($) {
    var reportName = null;
    var GetReport = function () {
        var times = [null, null];
        var dt, $this;
        $('div.input-daterange input:text').each(function (index) {
            $this = $(this);
            if (dt = $this.datepicker('getDate')) {
                if (!window.isNaN(dt.getTicks())) {
                    times[index] = dt.format('M/d/yyyy');
                }
            }
        });
        
        var leadType = +$('#leadType option:selected').val();
        var params = {
            postData: { name: reportName, startTime: times[0], endTime: times[1], leadType: leadType, page: 1 },
            datatype: 'json'
        };
        $('#reportingGrid').jqGrid('setGridParam', params).trigger('reloadGrid', [{ page: 1 }]);
    };

    var loadGrid = function (columns, params) {
        $('#reportingGrid').jqGrid({
            url: '/ajax/GetReport',
            ajaxGridOptions: { contentType: 'application/json; charset=utf-8' },
            postData: params,
            datatype: 'json',
            mtype: 'POST',
            serializeGridData: function(data) {
                var obj = { name: reportName, startTime: null, endTime: null, leadType: null, page: null, rows:null };
                for (var prop in obj) {
                    if (data[prop] != undefined) {
                        obj[prop] = data[prop];
                    }
                }
                return JSON.stringify(obj);
            },
            autowidth: true,
            viewrecords: true,
            height: '100%',
            emptyrecords:'No records found',
            colModel: columns,
            pager: $('#gridpager'),
            loadonce: false,
            hoverrows: false,
            sortable: false,
            cmTemplate: { sortable: false },
            //sortname: 'CreateDate',
            //sortorder: "desc",
            multipleSearch: false,
            caption: "Customer Report",
            page: 1,
            rowNum: 30,
            jsonReader: {
                root: 'Report',
                repeatitems: false,
                page: function (data) {
                    if(data.PageInfo) {
                        return data.PageInfo.PageNum;
                    } else {
                        return 0;
                    }
                },
                total: function (data) {
                    if (data.PageInfo) {
                        return data.PageInfo.PageCount;
                    } else {
                        return 0;
                    }
                },
                records: function (data) {
                    if (data.PageInfo) {
                        return data.PageInfo.TotalRecords;
                    } else {
                        return 0;
                    }
                }
            }, loadComplete: function (data) {
                var div = $('#reportingGrid').prev('div');
                if (data.Report.length === 0) {
                    if (div.find('.norecords').length === 0) {
                        div.append($('<h2>', { 'class': 'norecords bg-danger', text: 'No records found' }));
                    }
                } else {
                    div.find('.norecords').remove();
                }
            }/*,
                        onPaging: function() {
                            $(this).setGridParam({ datatype: 'json' }).triggerHandler("reloadGrid");
                        },
                        loadComplete: function (data) {
                            var $this = $(this);
                            if ($this.jqGrid('getGridParam', 'datatype') === 'json') {
                                // because one use repeatitems: false option and uses no
                                // jsonmap in the colModel the setting of data parameter
                                // is very easy. We can set data parameter to data.rows:
                                $this.jqGrid('setGridParam', {
                                    datatype: 'local',
                                    data: data.LeadReport,
                                    pageServer: data.PageInfo.PageNum,
                                    recordsServer: data.PageInfo.PageCount,
                                    lastpageServer: data.PageInfo.TotalRecords
                                });

                                // because we changed the value of the data parameter
                                // we need update internal _index parameter:
                                this.refreshIndex();

                                if ($this.jqGrid('getGridParam', 'sortname') !== '') {
                                    // we need reload grid only if we use sortname parameter,
                                    // but the server return unsorted data
                                    $this.triggerHandler('reloadGrid');
                                }
                            } else {
                                $this.jqGrid('setGridParam', {
                                    page: $this.jqGrid('getGridParam', 'pageServer'),
                                    records: $this.jqGrid('getGridParam', 'recordsServer'),
                                    lastpage: $this.jqGrid('getGridParam', 'lastpageServer')
                                });
                                this.updatepager(false, true);
                            }
                        }*/
        }).navGrid('#gridpager', { edit: false, add: false, del: false, search: false })
            .navButtonAdd('#gridpager', {
                caption: 'Excel',
                buttonicon: "ui-icon-disk",
                position: 'last',
                onClickButton: function () {
                    var grid = $('#reportingGrid');
                    var postData = grid.jqGrid('getGridParam', 'postData');
                    //var maxRows = grid.jqGrid('getGridParam', 'records');
                    $.ajaxRequest('GetReport', { name: reportName, startTime: postData.startTime, endTime: postData.endTime }, function (data) {
                        data.Report.exportToCSV();
                    });
                }
            });
    };

    $(document).ready(function () {
        var today = new Date();
        var yesterday = new Date(today.getFullYear(), today.getMonth(), today.getDate() - 1, 0, 0, 0, 0);

        var maxDate = new Date(today);
        maxDate.setDate(today.getDate() + 1);

        $('#leadType option:first').prop('selected', true);
        $('div.input-daterange').datepicker({
            maxDate: today,
            autoclose: true,
            clearBtn:true,
            todayHighlight: true
        }).on('changeDate', GetReport);

        $('.input-daterange input:text').each(function (index) {
            $(this).datepicker('setDate', yesterday);
        });

        reportName = $('#jqgrid').attr('data-report');
        $.getJSON('/Scripts/jqGrid-Columns/{0}.json?{1}'.format(reportName, today.getTicks()), function (results) {
            var params = { startTime: yesterday, endTime: yesterday };
            loadGrid(results, params);
        });
    }).on('change', '#leadType', GetReport);
})(jQuery);