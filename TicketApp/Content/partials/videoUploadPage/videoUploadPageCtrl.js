app.controller('videoUploadPageCtrl', function ($scope, $http) {

    $scope.getVideo = function () {

        //First, lets try to get the file from the user 

        var file = document.getElementById('file').files[0],
        reader = new FileReader();

        reader.onloadend = function (event) {
            var videoData = event.target.result;
            //send your binary data via $http or $resource or do anything else with it
           var stringVideo = String.fromCharCode.apply(null, new Uint8Array(videoData))


            $http({
                url: "Models/Services/videoUpload.asmx/uploadVideo",
                type: "POST",
                data: JSON.stringify({ str: stringVideo }),
                dataType: 'JSON',
                contentType: "application/json; charset=utf-8",
            });

           
        } //End of onboard function 

        reader.readAsArrayBuffer(file)

        
    };



    // Get the modal
    var modal = document.getElementById('uploadModal');

    // Get the button that opens the modal
    var btn = document.getElementById("myBtn");

    // Get the <span> element that closes the modal
    var span = document.getElementsByClassName("close")[0];

    // When the user clicks on the button, open the modal 
    //btn.onclick = function () {
    //    modal.style.display = "block";
    //}

    // When the user clicks on <span> (x), close the modal
    //span.onclick = function () {
    //    modal.style.display = "none";
    //}

    // When the user clicks anywhere outside of the modal, close it
    window.onclick = function (event) {
        if (event.target == modal) {
            modal.style.display = "none";
        }
    }


});