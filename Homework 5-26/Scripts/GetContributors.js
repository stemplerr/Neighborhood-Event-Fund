$(function () {
    $('#add-contributor-button').on('click', function () {
        $('#add-contributor-modal').modal();
        $('#add-contributor-form').attr('action', "/contributors/addcontributor");
        $('#modal-title').html('Add Contributor');
        $('#initial-deposit').show();
        $('#initial-deposit-label').show();
        $('#submit-new-contributor').show();
        $('#submit-updated-contributor').hide();

        $("#contributor-id-edit").val('');
        $('.firstname').val('');
        $(".lastname").val('');
        $(".cellnumber").val('');
        $('.alwaysinclude').val(false);
    });
    $('.edit-contributor-button').on('click', function () {
        $('#add-contributor-modal').modal();
        $('#add-contributor-form').attr('action', "/contributors/editcontributor");
        $('#modal-title').html('Edit Contributor');
        $('#initial-deposit').hide();
        $('#initial-deposit-label').hide();
        $('#submit-new-contributor').hide();
        $('#submit-updated-contributor').show();

        var contribId = $(this).data('contributor-id');
        var firstName = $(this).data('first-name');
        var lastName = $(this).data('last-name');
        var cell = $(this).data('cell-number');
        var alwaysInclude = $(this).data('always-include');

        $("#contributor-id-edit").val(contribId);
        $('.firstname').val(firstName);
        $(".lastname").val(lastName);
        $(".cellnumber").val(cell);
        $('#alwaysinclude').attr('checked', alwaysInclude === 'True');
    });

    $('.deposit-button').on('click', function () {
        $('#deposit-modal').modal();
        $('#modal-amount').val('');
        var name = $(this).data('contributor-name');
        $('.deposit-modal-title').text('Make A Deposit For ' + name);
        $('#modal-deposit-button').data('contributor-id', $(this).data('contributor-id'));
    });

    

    $('#modal-deposit-button').on('click', function () {
        var self = $(this);
        var id = self.data('contributor-id');
        var amount = $('#modal-amount').val();
        $.post('/contributors/deposit', { id: id, amount: amount }, function (newAmount) {
            console.log(newAmount);
            var row = $('tr[data-contributor-id="' + id + '"]');
            row.find('td:eq(2)').text('$' + newAmount);
            $('#deposit-modal').modal('hide');
        });
        
    });
});