$(function () {

    //связывание select'ов
    $('div.CELListBoxesPanel').each(function () {
        var linkedSelectsObj = new linkedSelects(
            $(this).find('select.CELLeftListBox')[0],
            $(this).find('select.CELRightListBox')[0],
            $(this).find('input.CELAddButton')[0],
            $(this).find('input.CELRemoveButton')[0],
            $(this).find('div.CELHiddenFieldWrap input')[0]);
        this.linkedSelectsObj = linkedSelectsObj;
    });

});