// Angular Module
angular.module('QuizApp', [])
    .controller('QuizCtrl', function ($scope, $http) {
        $scope.answered = false;
        $scope.title = "loading question...";
        $scope.options = [];
        $scope.correctAnswer = false;
        $scope.working = false;

        $scope.answer = function () {
            return $scope.correctAnswer ? 'correct' : 'incorrect';
        };

        $scope.RefreshUI = function () {
            //loop 
            $("[name='poll_bar'").each(
                    function (i) {
                        //get poll value 	
                        var bar_width = (parseFloat($("[name='poll_val'").eq(i).text()) / 2).toString();
                        bar_width = bar_width + "%"; //add percentage sign.										
                        //set bar button width as per poll value mention in span.
                        $("[name='poll_bar'").eq(i).width(bar_width);

                        //Define rules.
                        var bar_width_rule = parseFloat($("[name='poll_val'").eq(i).text());
                        if (bar_width_rule >= 50) { $("[name='poll_bar'").eq(i).addClass("btn btn-sm btn-success") }
                        if (bar_width_rule < 50) { $("[name='poll_bar'").eq(i).addClass("btn btn-sm btn-warning") }
                        if (bar_width_rule <= 10) { $("[name='poll_bar'").eq(i).addClass("btn btn-sm btn-danger") }

                        //Hide drill down divs
                        $("#" + $("[name='poll_bar'").eq(i).text()).hide();
                    });
        };

        $scope.nextQuestion = function () {
            $scope.working = true;
            $scope.answered = false;
            $scope.title = "loading question...";
            $scope.options = [];

            $http.get("/api/trivia").success(function (data, status, headers, config) {
                $scope.options = data.options;
                $scope.title = data.title;
                $scope.answered = false;
                $scope.working = false;
            }).error(function (data, status, headers, config) {
                $scope.title = "Oops... something went wrong";
                $scope.working = false;
            });
            $scope.RefreshUI();
        };

        $scope.sendAnswer = function (option) {
            $scope.working = true;
            $scope.answered = true;

            $http.post('/api/trivia', { 'questionId': option.questionId, 'optionId': option.id }).success(function (data, status, headers, config) {
                $scope.correctAnswer = (data.result === true);
                $scope.statisticDate = moment(data.statistic.date,'', 'de').format('LL');
                var totalAnswered = data.statistic.passed + data.statistic.failed;
                $scope.statisticFailedCount = data.statistic.failed;
                $scope.statisticPassedCount = data.statistic.passed;

                if (totalAnswered > 0) {
                    $scope.statisticFailedPercent = (data.statistic.failed / totalAnswered) * 100;
                    $scope.statisticPassedPercent = (data.statistic.passed / totalAnswered) * 100;
                }
                else {
                    $scope.statisticFailedPercent = data.statistic.failed;
                    $scope.statisticPassedPercent = data.statistic.passed;
                }
                $scope.working = false;
            }).error(function (data, status, headers, config) {
                $scope.title = "Oops... something went wrong";
                $scope.working = false;
            });
            $scope.RefreshUI();
        };
    });