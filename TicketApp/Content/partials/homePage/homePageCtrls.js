app.controller('homePageCtrls', function ($scope, $http) {


   $scope.FindInfo = function () {
        $http
         ({
             method: "GET",
             url: "Models/Services/cacInformation.asmx/findInfo",
             //data: JSON.stringify({ server: $scope.server }),
             contentType: "application/json; charset=utf-8",
             dataType: 'JSON',
         }).then(function (response) {
             //location.reload() 
             $scope.personInfo = response.data;
            // $rootScope.account = response.data;

         }
      )
   };

    // Get the modal
   var modal = document.getElementById('myModal');

    // Get the button that opens the modal
   var btn = document.getElementById("myBtn");

    // Get the <span> element that closes the modal
   var span = document.getElementsByClassName("close")[0];

    // When the user clicks on the button, open the modal 
   btn.onclick = function () {
       modal.style.display = "block";
   }

    // When the user clicks on <span> (x), close the modal
   span.onclick = function () {
       modal.style.display = "none";
   }

    // When the user clicks anywhere outside of the modal, close it
   window.onclick = function (event) {
       if (event.target == modal) {
           modal.style.display = "none";
       }
   }

});