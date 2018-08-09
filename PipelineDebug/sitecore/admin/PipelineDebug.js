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
        $('#discovery').on('click', '.toggle', discoveryOverlay.toggleChildren);
        $('#discovery').on('click', 'input.custom', discoveryOverlay.addCustom);
        $('#discovery').on('click', 'input.addtaxonomy', discoveryOverlay.toggleTaxonomy);
        $('#discovery').on('click', 'input.selectedtaxonomy', discoveryOverlay.toggleSelectedTaxonomy);
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
        var container = $('#discovery');
        container.empty();
        container.append('<h2>Selected</h2>');
        container.append(discoveryOverlay.renderTaxonomies(result.Taxonomies));
        container.append('<h2>Discovery</h2>');
        container.append(discoveryOverlay.renderDiscoveryRoots(result));
    },
    renderTaxonomies: function (taxonomies) {
        return '<div class="selected">' +
            $.map(taxonomies, function (taxonomy) { return discoveryOverlay.renderTaxonomy(taxonomy); }).join('') +
            discoveryOverlay.renderCustomRow() +
            '</div>';
    },
    renderTaxonomy: function (taxonomy) {
        return '<div>' +
            '<input type="checkbox" class="selectedtaxonomy" checked="checked" value="' + taxonomy + '" />' +
            taxonomy +
            '</div>';
    },
    renderCustomRow: function () {
        return '<div>' +
            '<input type="checkbox" class="selectedtaxonomy custom" />' +
            '<input type="text" placeholder="add a custom taxonomy" />' +
            '</div>';
    },
    renderDiscoveryRoots: function (result) {
        return '<div class="discoveryroots">' +
            $.map(result.DiscoveryRoots, function (root, i) { return discoveryOverlay.renderSubtree(root, result.Taxonomies, false, null); }).join('') +
            '</div>';
    },
    renderSubtree: function (discoveryItem, taxonomies, expand, parentType) {
        var children = '', checked = '', icons = '';

        var css = new Array();
        if (expand) {
            css.push('opened');
        }
        if (discoveryItem.HasToString) {
            css.push('selectable');
        }

        if (!discoveryItem.IsPrimitive) {
            icons += '<img src="/sitecore/shell/themes/standard/images/treemenu_expanded.png" class="icon toggle closed" />';
            icons += '<img src="/sitecore/shell/themes/standard/images/treemenu_collapsed.png" class="icon toggle opened" />';
        } else {
            icons += '<img src="/sitecore/shell/themes/standard/images/noexpand15x15.gif" class="icon" />';
        }

        if ($.inArray(discoveryItem.Taxonomy, taxonomies) >= 0) {
            checked += 'checked';
        }
        icons += '<input type="checkbox" class="addtaxonomy" ' + checked + ' value="' + discoveryItem.Taxonomy +'" />';

        if (discoveryItem.ProtectionLevel.toLowerCase() === 'public') {
            icons += '<img src="/~/icon/office/16x16/lock_open.png" class="icon" title="public" />';
        } else {
            icons += '<img src="/~/icon/office/16x16/lock.png" class="icon private" title="' + discoveryItem.ProtectionLevel + '" />';
        }

        if (discoveryItem.MemberType.toLowerCase() === 'property') {
            icons += '<img src="/~/icon/office/16x16/gearwheels.png" class="icon property" title="Property" />';
        } else {
            icons += '<img src="/~/icon/office/16x16/document_text.png" class="icon field" title="' + discoveryItem.MemberType + '" />';
        }

        if (parentType === null || parentType === discoveryItem.DeclaringType) {
            icons += '<img src="/~/icon/office/16x16/document_empty.png" class="icon local" title="local member of ' + discoveryItem.DeclaringType + '" />';
        } else {
            icons += '<img src="/~/icon/office/16x16/copy_to.png" class="icon derived" title="derived from ' + discoveryItem.DeclaringType + '" />';
        }
        
        if (discoveryItem.Members !== null && discoveryItem.Members.length > 0) {
            children += '<div class="children">';
            children += $.map(discoveryItem.Members, function (item, i) { return discoveryOverlay.renderSubtree(item, taxonomies, false, discoveryItem.TypeFullName); }).join('');
            children += '</div>';
        }

        return '<div title="' + discoveryItem.Taxonomy + '" class="discovery ' + css.join(' ') + '">' +
            icons +
            discoveryItem.Name + ' <span class="typename" title="' + util.htmlEncode(discoveryItem.TypeFullName) + '">' + util.htmlEncode(discoveryItem.TypeName) + '</span>' +
            children +
            '</div>';
    },
    show: function () {
        $('#discovery').dialog({
            modal: true,
            width: 800,
            height: 600,
            buttons: {
                Cancel: function () { $(this).dialog("close"); },
                Save: discoveryOverlay.save
            }
        });
    },
    toggleChildren: function () {
        var element = $(this).closest('.discovery');
        var children = element.children('.children');
        if (children.length === 1) {
            if (element.hasClass('opened')) {
                element.removeClass('opened');
                element.addClass('closed');
            } else {
                element.removeClass('closed');
                element.addClass('opened');
            }
        } else {
            discoveryOverlay.discover(element);
        }
    },
    addCustom: function () {
        if ($('.selected input.custom:not(:checked)').length === 0) {
            $('.selected').append(discoveryOverlay.renderCustomRow());
        }
    },
    toggleTaxonomy: function () {
        var taxonomy = $(this).val();
        var input = $('.selected input[value="' + taxonomy + '"]');
        if (input.length === 0) {
            $('.selected').append(discoveryOverlay.renderTaxonomy(taxonomy));
        } else {
            input.prop('checked', $(this).is(':checked'));
        }
    },
    toggleSelectedTaxonomy: function () {
        var taxonomy = discoveryOverlay.selectedTaxonomyValue(this);
        $('.discoveryroots input[value="' + taxonomy + '"]').prop('checked', $(this).is(':checked'));
    },
    selectedTaxonomyValue: function (elem) {
        if ($(elem).hasClass('custom')) {
            return $(elem).siblings('input[type=text]').val();
        } 
        return $(elem).val();
    },
    discover: function (element) {
        var taxonomy = $(element).attr('title');
        var taxonomies = new Array();
        if ($(element).find('input').is(':checked')) {
            taxonomies.push(taxonomy);
        }
        var data = { ProcessorId: $('#discovery').data('processorid'), Taxonomy: taxonomy };
        service.update('discover', data, function (result) { $(element).replaceWith(discoveryOverlay.renderSubtree(result.DiscoveryItem, taxonomies, true, null)); });
    },
    save: function () {
        var taxonomies = $.map($('.selected input:checked'), function (input) { return discoveryOverlay.selectedTaxonomyValue(input); });
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
        $('#defaultTaxonomies').val(result.Settings.DefaultTaxonomies);
    },
    save: function () {
        var data = { SessionOnly: $('#sessionConstraint').is(':checked'), Site: $('#siteConstraint').val(), Language: $('#languageConstraint').val(), IncludeUrlPattern: $('#includeUrlConstraint').val(), ExcludeUrlPattern: $('#excludeUrlConstraint').val(), LogToDiagnostics: $('#enableFileLogging').is(':checked'), LogToMemory: $('#enableMemoryLogging').is(':checked'), MaxMemoryEntries: $('#maxEntries').val(), MaxEnumerableIterations: $('#maxIterations').val(), DefaultTaxonomies: $('#defaultTaxonomies').val() };
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
        var entries = $.map(item.Entries, function (entry) { return '<span class="output entry"><strong>' + entry.Taxonomy + ':</strong> ' + entry.Value + '</span>'; }).join('<br />');
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