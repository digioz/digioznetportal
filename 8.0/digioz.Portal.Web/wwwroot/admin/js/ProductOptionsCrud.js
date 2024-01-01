$(document).ready(function () {

    // For Sizes
    $("#AddSize").click(function () {
        var newSize = $("#SizeNew").val();

        if (newSize != null && newSize != "") {
            $("#SizeList").append($('<option>', {
                value: newSize,
                text: newSize
            }));

            $("#SizeNew").val("");
        }
    });

    $("#RemoveSize").click(function () {
        //alert("you clicked!");
        $("#SizeList > option:selected").remove();
    });

    // For Colors
    $("#AddColor").click(function () {
        var newColor = $("#ColorNew").val();

        if (newColor != null && newColor != "") {
            $("#ColorList").append($('<option>', {
                value: newColor,
                text: newColor
            }));

            $("#ColorNew").val("");
        }
    });

    $("#RemoveColor").click(function () {
        //alert("you clicked!");
        $("#ColorList > option:selected").remove();
    });

    // For Material Types
    $("#AddMaterialType").click(function () {
        var newMaterialType = $("#MaterialTypeNew").val();

        if (newMaterialType != null && newMaterialType != "") {
            $("#MaterialTypeList").append($('<option>', {
                value: newMaterialType,
                text: newMaterialType
            }));

            $("#MaterialTypeNew").val("");
        }
    });

    $("#RemoveMaterialType").click(function () {
        $("#MaterialTypeList > option:selected").remove();
    });

    // select all on button submit
    $("#submit").click(function () {
        $("#SizeList option").each(function () {
            $(this).attr('selected', true);
        });
        $("#ColorList option").each(function () {
            $(this).attr('selected', true);
        });
        $("#MaterialTypeList option").each(function () {
            $(this).attr('selected', true);
        });
    });
});