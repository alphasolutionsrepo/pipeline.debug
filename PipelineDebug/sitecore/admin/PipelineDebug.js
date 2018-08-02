var pipelineDebug = {
    init: function () {
        pipelineDebug.initMenu();
        login.init();
        pipelines.init();
        processors.init();
        settings.init();
        output.init();
        importexport.init();
        discoveryOverlay.init();
        pipelines.load();
    },
    displayError: function (error) {
        alert(error);
    },
    displayPage: function (selector) {
        $('.subpage').hide();
        $(selector).show();
    },
    initMenu: function () {
        $(document).on('click', '#menupipelines', pipelines.show);
        $(document).on('click', '#menuprocessors', processors.load);
        $(document).on('click', '#menusettings', settings.load);
        $(document).on('click', '#menuoutput', output.load);
        $(document).on('click', '#menuexport', importexport.load);
    }
};

var login = {
    page: '#login',
    init: function () {
        $(document).on('click', '#menulogout', login.logout);
        $(login.page).on('click', '#loginButton', login.login);
    },
    show: function () {
        pipelineDebug.displayPage(login.page);
    },
    login: function () {
        var data = { Username: $('#login input[name=username]').val(), Password: $('#login input[name=password]').val() };
        service.update('login', data, pipelines.load);
    },
    logout: function () {
        service.update('logout', null, login.show);
    }
};

var pipelines = {
    page: '#pipelines',
    init: function () {
        $(pipelines.page).on('change', '#pipelineList', pipelines.select);
        $(pipelines.page).on('click', '.processorlist .settings', function () { discoveryOverlay.load(this); });
        $(pipelines.page).on('click', '.processorlist .delete', function () { processors.deleteProcessor(this, pipelines.loadDetails); });
    },
    load: function () {
        service.update('listpipelines', null, pipelines.renderAndShow);
    },
    renderAndShow: function (result) {
        pipelines.render(result);
        pipelines.show();
    },
    render: function (result) {
        $.each(result.PipelineGroups, function (group, pipelines) { $('#pipelineList').append('<optgroup label="' + group + '">' + $.map(pipelines, function (pipeline, j) { return '<option>' + pipeline + '</option>'; }).join('') + '</optgroup>'); });
        $('#pipelineList').chosen({ 'width': '840px' });
    },
    show: function () {
        pipelineDebug.displayPage(pipelines.page);
    },
    select: function () {
        var selected = $(this).find('option:selected');
        var details = $('#pipelineDetails');
        details.data('group', selected.parent().attr('label'));
        details.data('pipeline', selected.text());
        details.empty();
        pipelines.loadDetails();
    },
    loadDetails: function () {
        var details = $('#pipelineDetails');
        var data = { Group: details.data('group'), Name: details.data('pipeline') };
        service.update('pipelinedetails', data, pipelines.renderDetails);
    },
    renderDetails: function (result) {
        var details = $('#pipelineDetails');
        details.html('<ul class="processorlist"><li id="addProcessor" class="ui-state-highlight">Add new DebugProcessor (drag and drop)</li></ul><ul id="pipelineProcessors" class="processorlist"></ul>');
        var container = details.find('#pipelineProcessors');
        container.html($.map(result.Processors, processors.renderProcessor).join(''));
        container.sortable({
            cancel: 'li:not(.ui-state-highlight)',
            revert: true,
            update: pipelines.addMove
        });
        details.find('#addProcessor').draggable({
            connectToSortable: '#pipelineProcessors',
            helper: 'clone',
            revert: 'invalid'
        });
        details.find("ul, li").disableSelection();
    },
    addMove: function (event, ui) {
        if (ui.item.data('processorid') === undefined) {
            var data = { Group: $('#pipelineDetails').data('group'), Name: $('#pipelineDetails').data('pipeline'), Index: ui.item.index() };
            service.update('addprocessor', data, pipelines.renderDetails);
            return;
        } else {
            var data = { ProcessorId: ui.item.data('processorid'), NewIndex: ui.item.index() };
            service.update('moveprocessor', data, pipelines.renderDetails);
            return;
        }
    }
};

var processors = {
    page: '#processors',
    init: function () {
        $(processors.page).on('click', '.processorlist .settings', function () { discoveryOverlay.load(this); });
        $(processors.page).on('click', '.processorlist .delete', function () { processors.deleteProcessor(this, processors.load); });
    },
    load: function () {
        service.update('getdebugprocessors', null, processors.renderAndShow);
    },
    renderAndShow: function (result) {
        processors.render(result);
        processors.show();
    },
    render: function (result) {
        var innerHtml = $.map(result.DebugProcessors, processors.renderProcessor).join('');
        $('#debugProcessors').empty().html('<ul class="processorlist">' + innerHtml + '</ul>');
    },
    show: function () {
        pipelineDebug.displayPage(processors.page);
    },
    renderProcessor: function (processor) {
        if (processor.ProcessorId === null || processor.ProcessorId === '') {
            return '<li class="ui-state-default">' + processor.Name + '</li>';
        }
        else {
            return '<li class="ui-state-default ui-state-highlight" data-processorid="' + processor.ProcessorId + '">' +
                processor.Name +
                '<img src="/~/icon/office/16x16/delete.png" class="icon delete" title="delete processor" />' +
                '<img src="/~/icon/office/16x16/gearwheel.png" class="icon settings" title="edit processor settings" />' +
                '</li>';
        }
    },
    editProcessor: function () {
        var id = $(element).closest('li').data('processorid');
        var data = { ProcessorId: id };
    },
    deleteProcessor: function (element, callback) {
        var id = $(element).closest('li').data('processorid');
        var data = { ProcessorId: id };
        service.update('removeprocessor', data, callback);
    }
};

var discoveryOverlay = {
    init: function () {
        $('#discovery').on('click', '.discoverable', function () { discoveryOverlay.discover(this); });
        $('#discovery').on('click', '.discoverable input', function (event) { event.stopPropagation(); });
    },
    load: function (element) {
        var id = $(element).closest('li').data('processorid');
        $('#discovery').data('processorid', id);
        var data = { ProcessorId: id };
        service.update('getdiscoveryroots', data, discoveryOverlay.renderAndShow);
    },
    renderAndShow: function (result) {
        discoveryOverlay.render(result);
        discoveryOverlay.show();
    },
    render: function (result) {
        $('#discovery').empty().html($.map(result.DiscoveryRoots, function (root, i) { return discoveryOverlay.renderSubtree(root, result.Taxonomies); }).join(''));
    },
    renderSubtree: function (discoveryItem, taxonomies) {
        var children = '', checked = '', icons = '';

        var css = new Array();
        if (!discoveryItem.IsPrimitive && discoveryItem.Members === null) {
            css.push('discoverable');
        }
        if (discoveryItem.HasToString) {
            css.push('selectable');
        }

        if (discoveryItem.ProtectionLevel === 'Public') {
            icons += '<img src="/~/icon/office/16x16/lock_open.png" title="Public" />';
        } else {
            icons += '<img src="/~/icon/office/16x16/lock.png" class="icon private" title="' + discoveryItem.ProtectionLevel + '" />';
        }

        if (discoveryItem.MemberType === 'Property') {
            icons += '<img src="/~/icon/office/16x16/gearwheels.png" class="icon property" title="Property" />';
        } else {
            icons += '<img src="/~/icon/office/16x16/document_text.png" class="icon field" title="' + discoveryItem.MemberType + '" />';
        }

        if ($.inArray(discoveryItem.Taxonomy, taxonomies) >= 0) {
            checked += 'checked';
        }

        if (discoveryItem.Members !== null && discoveryItem.Members.length > 0) {
            children += '<div class="children">';
            children += $.map(discoveryItem.Members, function (item, i) { return discoveryOverlay.renderSubtree(item, taxonomies); }).join('');
            children += '</div>';
        }

        return '<div data-taxonomy="' + discoveryItem.Taxonomy + '" class="discovery ' + css.join(' ') + '">' +
            '<input type="checkbox" title="' + discoveryItem.Taxonomy + '" class="addtaxonomy" ' + checked + ' />' +
            icons +
            util.htmlEncode(discoveryItem.TypeName) + ' ' + discoveryItem.Name +
            children +
            '</div>';
    },
    show: function () {
        $('#discovery').dialog({
            modal: true,
            width: 800,
            height: 600,
            buttons: { Save: discoveryOverlay.save }
        });
    },
    discover: function (element) {
        var taxonomy = $(element).data('taxonomy');
        var taxonomies = new Array();
        if ($(element).find('input').is(':checked')) {
            taxonomies.push(taxonomy);
        }
        var data = { ProcessorId: $('#discovery').data('processorid'), Taxonomy: taxonomy };
        service.update('discover', data, function (result) { $(element).replaceWith(discoveryOverlay.renderSubtree(result.DiscoveryItem, taxonomies)); });
        return;
    },
    save: function () {
        var taxonomies = $.map($('#discovery .selectable > input:checked'), function (input) { return $(input).attr('title'); });
        var data = { ProcessorId: $('#discovery').data('processorid'), Taxonomies: taxonomies };
        service.update('saveprocessortaxonomies', data, null);
        $(this).dialog("close");
    }
};

var settings = {
    page: '#settings',
    init: function () {
        $(settings.page).on('click', '#saveSettings', settings.save);
    },
    load: function () {
        service.update('getsettings', null, settings.renderAndShow);
    },
    renderAndShow: function (result) {
        settings.render(result);
        settings.show();
    },
    show: function () {
        pipelineDebug.displayPage(settings.page);
    },
    render: function (result) {
        $('#sessionConstraint').prop('checked', result.Settings.SessionOnly);
        $('#siteConstraint').val(result.Settings.Site);
        $('#languageConstraint').val(result.Settings.Language);
        $('#includeUrlConstraint').val(result.Settings.IncludeUrlPattern);
        $('#excludeUrlConstraint').val(result.Settings.ExcludeUrlPattern);

        $('#enableFileLogging').prop('checked', result.Settings.LogToDiagnostics);
        $('#enableMemoryLogging').prop('checked', result.Settings.LogToMemory);
        $('#maxEntries').val(result.Settings.MaxMemoryEntries);
        $('#maxIterations').val(result.Settings.MaxEnumerableIterations);
    },
    save: function () {
        var data = { SessionOnly: $('#sessionConstraint').is(':checked'), Site: $('#siteConstraint').val(), Language: $('#languageConstraint').val(), IncludeUrlPattern: $('#includeUrlConstraint').val(), ExcludeUrlPattern: $('#excludeUrlConstraint').val(), LogToDiagnostics: $('#enableFileLogging').is(':checked'), LogToMemory: $('#enableMemoryLogging').is(':checked'), MaxMemoryEntries: $('#maxEntries').val(), MaxEnumerableIterations: $('#maxIterations').val() };
        service.update('savesettings', data, null);
    }
};

var output = {
    page: '#output',
    unchecked: new Array(),
    init: function () {
        $(output.page).on('click', '#outputReload', output.load);
    },
    load: function () {
        output.unchecked = $.map($('#processorlist input[type=checkbox]:not(:checked)'), function (cb) { return $(cb).val(); });
        var data = { FilterProcessorIds: output.unchecked };
        service.update('getoutput', data, output.renderAndShow);
    },
    renderAndShow(result) {
        output.render(result);
        output.show();
    },
    show() {
        pipelineDebug.displayPage(output.page);
    },
    render(result) {
        output.renderProcessors(result);
        output.renderOutput(result);
    },
    renderProcessors(result) {
        $('#processorlist').html($.map(result.DebugProcessors, output.renderProcessor).join(''));
    },
    renderProcessor(processor) {
        var isChecked = $.inArray(processor.ProcessorId, output.unchecked) < 0;
        var checked = isChecked ? ' checked="checked" ' : '';
        return '<input type="checkbox" id="cb' + processor.ProcessorId + '" value="' + processor.ProcessorId + '"' + checked + ' /><label for="cb' + processor.ProcessorId + '">' + processor.Name + '</label><br />';
    },
    renderOutput(result) {
        var entries =
            $('#outputdetails').html($.map(result.Output, output.renderOutputItem).join(''));
    },
    renderOutputItem(item) {
        var entries = $.map(item.Entries, function (entry) { return '<span class="output entry">' + entry.Taxonomy + ': ' + entry.Value + '</span>'; }).join('<br />');
        return '<div><span class="output time">' + new Date(item.Time).toLocaleTimeString() + '</span> <span class="output name">' + item.ProcessorName + '</span><span class="output type">(' + item.ArgsType + ')</span><br />' + entries + '</div>';
    }
};

var importexport = {
    page: '#importexport',
    init: function () {
        $(importexport.page).on('click', '#triggerImport', importexport.import);
    },
    load: function () {
        service.update('exportconfiguration', null, importexport.renderAndShow);
    },
    renderAndShow(result) {
        importexport.render(result);
        importexport.show();
    },
    show() {
        pipelineDebug.displayPage(importexport.page);
    },
    render(result) {
        $('#exportConfiguration').val(JSON.stringify(result.Configuration));
    },
    import() {
        var data = JSON.parse($('#importConfiguration').val());
        if (data.Settings === undefined) {
            alert('It seems the format is incorrect');
            return;
        }
        service.update('importconfiguration', data, processors.load);
    }
};

var service = {
    update: function (method, data, display, showloading) {
        $('#loading').dialog({ modal: true, show: 300, width: 200, height: 200 });
        $.ajax({
            url: '/pipelinedebug/' + method,
            data: JSON.stringify(data),
            method: data === null ? 'GET' : 'POST',
            contentType: "application/json; charset=utf-8",
            dataType: "json"
        })
        .done(function (response) {
            if (response === null || response === '' || response.Status === null || response.Status === '') {
                $('#loading').dialog('close');
                pipelineDebug.displayError("Unexpected response");
                return;
            }
            if (response.Status === 'Error') {
                $('#loading').dialog('close');
                pipelineDebug.displayError(response.ErrorMessage);
                return;
            }
            if (response.Status === 'Unauthorized') {
                $('#loading').dialog('close');
                login.show();
                return;
            }
            if (response.Status === 'Success') {
                if (display) {
                    try {
                        display(response);
                    }
                    catch (err) {
                        pipelineDebug.displayError(err.message);
                    }
                }

                $('#loading').dialog('close');
                return;
            }
            pipelineDebug.displayError('Unexpected response Status');
        })
        .fail(function (error) {
            $('#loading').dialog('close');
            pipelineDebug.displayError("Server error");
        });
    }
};

var util = {
    htmlEncode: function (value) {
        return $('<div/>').text(value).html();
    }
};

$(function () {
    pipelineDebug.init();
});