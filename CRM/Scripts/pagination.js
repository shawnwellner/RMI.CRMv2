(function ($) {
    var pageInfo = null;

    $(document).ready(function () {
        //$('[data-provide="pagination"]').removeClass('hidden');
        $('#PageNum').val('1');
    }).on('change', '[data-provide="pagination"] .records-info .max-rows select', function () {
        $('#PageNum').val(1);
        $('#MaxRows').val(this.value);
        var panel = $('.panel-default:visible');
        panel.find('.panel-heading a:not(".collapsed")').click();
        panel.find('button:submit').click();
    }).on('click', '[data-provide="pagination"] ul.pagination li:not(".disabled")', function () {
        var $this = $(this);
        if ($this.is('ul.pagination li.back:first')) {
            pageInfo.PageNum = 1;
        } else if ($this.is('ul.pagination li.fwd:last')) {
            pageInfo.PageNum = pageInfo.PageCount;
        } else if ($this.is('ul.pagination li.back')) {
            pageInfo.PageNum--;
        } else if ($this.is('ul.pagination li.fwd')) {
            pageInfo.PageNum++;
        } else {
            pageInfo.PageNum = +$this.text();
        }
        $('.panel-heading a:not(".collapsed")').click();

        var maxRows = $('.records-info .max-rows select').val();
        $('#PageNum').val(pageInfo.PageNum);
        $('.panel-default:visible button:submit').click();
    }).ajaxStart(function (e) {
        //$('[data-provide="pagination"]').addClass('temp hidden');
    }).ajaxStop(function (e) {
        var $target = $('[data-provide="pagination"].temp').removeClass('temp');
        if (!pageInfo || pageInfo.PageCount <= 1) { return; }
        $target.removeClass('hidden');
    });

    $.fn.pagination = function (data) {
        if (typeof data === 'string') {
            return this.data(data);
        } else if (typeof data === 'object') {
            this.data('PageInfo', data);
            pageInfo = data;
            if (pageInfo && pageInfo.PageCount > 0) {
                var paginationUl = this.find('ul.pagination'); //.data('PageInfo', pageInfo);
                var recInfo = this.find('.records-info');
                recInfo.find('.page-count span').html(pageInfo.PageCount);
                recInfo.find('.total-records span').html(pageInfo.TotalRecords);
                switch (pageInfo.MaxRows) {
                    case 10:
                    case 20:
                    case 50:
                        recInfo.find('.max-rows select').val(pageInfo.MaxRows);
                        break;
                    default:
                        recInfo.find('.max-rows select').remove();
                }

                paginationUl.find('li').removeClass('disabled active');

                if (pageInfo.PageNum === 1) {
                    paginationUl.find('li.back').addClass('disabled');
                }
                if (pageInfo.PageNum >= pageInfo.PageCount) {
                    paginationUl.find('li.fwd').addClass('disabled');
                }

                var page = pageInfo.PageNum - 2;
                while (page < 1) { page++; }
                while (page + 5 > pageInfo.PageCount + 1) { page--; }

                paginationUl.find('li.page')
                            .each(function () {
                                var $this = $(this);
                                if (page < 1) {
                                    $this.addClass('hidden');
                                } else {
                                    $this.removeClass('hidden').find('span').html(page);
                                    if (page === pageInfo.PageNum) {
                                        $this.addClass('active');
                                    }
                                }
                                page++;
                            });
                this.find('#PageNum').val(pageInfo.PageNum);
                $('.export-controls .checkbox').removeClass('hidden');
                $('[data-provide="pagination"]').removeClass('hidden');
            } else {
                //$('[data-provide="pagination"]').addClass('hidden');
                //$('.export-controls .checkbox').addClass('hidden');
                $('#PageNum').val('1');
            }
        }
        return this;
    };
})(jQuery);