var app = angular.module('TicketApp', ['ngRoute']);

app.config(['$routeProvider', function dataConfig($routeProvider) {

    $routeProvider
      .when('/',
      {
          templateUrl: 'Content/partials/homePage/homePage.html',
          //controller: 'homePageDir'
      })
    $routeProvider
     .when('/videoUpload',
     {
         templateUrl: 'Content/partials/videoUploadPage/videoUploadPage.html',
         //controller: 'homePageDir'
     })
}]);