//03.05.2012

function linkedSelects(LeftSelect, RightSelect, AddButton, RemoveButton, HiddenField) {

    //events
    $(LeftSelect).change(function () {
        $(RightSelect).find('option').prop('selected', false);
        updateButtonsState();
    });
    $(LeftSelect).dblclick(function () {
        if (!this.parentNode.parentNode.disabled)
            $(AddButton).trigger('click');
    });
    $(RightSelect).change(function () {
        $(LeftSelect).find('option').prop('selected', false);
        updateButtonsState();
    });
    $(RightSelect).dblclick(function () {
        if (!this.parentNode.parentNode.disabled)
            $(RemoveButton).trigger('click');
    });
    $(AddButton).click(function () {
        moveOptions(LeftSelect, RightSelect);
        updateButtonsState();
        return false;
    });
    $(RemoveButton).click(function () {
        moveOptions(RightSelect, LeftSelect);
        updateButtonsState();
        return false;
    });

    //private methods
    function moveOption(FromSelect, ToSelect, optionIndex, select) {
        var option = FromSelect.options[optionIndex];
        var newOption = new Option(option.firstChild.nodeValue, option.value, false, select);
        newOption.title = option.firstChild.nodeValue;

        //определение позиции для вставки
        var i = 0;
        for (i = 0; i < ToSelect.options.length; ++i) {
            if (option.firstChild.nodeValue < ToSelect.options[i].text)
                break;
        }

        //вставка нового элемента в нужную позицию
        ToSelect.focus();
        if (ToSelect.options.length == i) {
            ToSelect.add(newOption);
        } else {
            try {
                ToSelect.add(newOption, ToSelect.options[i]);
            }
            //IE
            catch (ex) {
                ToSelect.add(newOption, i);
            }
        }
        
        FromSelect.remove(optionIndex);
    }

    //перемещение элементов из одного списка в другой
    function moveOptions(FromSelect, ToSelect) {
        //снятие выделения
        $(ToSelect).find('option').prop('selected', false);

        //перемещение
        for (var i = 0; i < FromSelect.options.length; ++i) {
            if (FromSelect.options[i].selected) {
                moveOption(FromSelect, ToSelect, i, true);
                --i;
            }
        }
        updateHiddenField();
    }

    function updateHiddenField() {
        HiddenField.value = '';
        for (var i = 0; i < RightSelect.options.length; ++i) {
            if (i > 0)
                HiddenField.value += ',';
            HiddenField.value += RightSelect.options[i].value;
        }
    }

    function fillRightSelect() {
        if (HiddenField.value == '')
            return;        
        
        var values = HiddenField.value.split(',');
        for (var i = 0; i < values.length; ++i) {
            for (var j = 0; j < LeftSelect.options.length; ++j) {
                if (LeftSelect.options[j].value == values[i]) {
                    moveOption(LeftSelect, RightSelect, j, false);
                }
            }
        }
    }

    //обновить состояние кнопок
    function updateButtonsState() {
        $(AddButton).prop('disabled', $(LeftSelect).find('option:selected').length == 0);
        $(RemoveButton).prop('disabled', $(RightSelect).find('option:selected').length == 0);
    }

    //public methods
    this.clearRightSelect = function() {
        $(RightSelect).find('option').prop('selected', true);
        moveOptions(RightSelect, LeftSelect);
        if (LeftSelect.options.length > 0) {
            $(LeftSelect).find('option').prop('selected', false);
        }
        updateButtonsState();
    };


    //код конструктора
    fillRightSelect();

    if (LeftSelect.options.length > 0 && $(LeftSelect).find('option:selected').length == 0 && !LeftSelect.disabled)
        LeftSelect.options[0].selected = true;

    updateButtonsState();

};
