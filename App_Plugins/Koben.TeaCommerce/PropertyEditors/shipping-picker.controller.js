angular.module("umbraco").controller("TeaCommerce.ShippingPickerController", function ($scope, $http) {



    if (!$scope.model.value) {
        $scope.model.value = {
            storeId: null,
            shippingMethodId: null
        }
    }


    $scope.refreshMethods = initShippingDropdown;

    function initStoreDropdown() {        
            $http.get('backoffice/teacommerce/stores/getall').success(function (data) {
                var entityFound = false;
                for (var i = 0; i < data.length; i++) {
                    //Must convert id to string because Umbraco saves and parse it back as a string
                    var entity = data[i];
                    entity.id = '' + entity.id;

                    if (entity.id == $scope.model.value.storeId) {
                        entityFound = true;
                        break;
                    }
                }

                $scope.stores = data;

                if ($scope.model.value.storeId && !entityFound) {
                    $http.get('backoffice/teacommerce/stores/get',
                        {
                            params: {
                                storeId: $scope.model.value.storeId
                            }
                        }).success(function (data) {
                            data.name = '* ' + data.name;
                            $scope.stores.push(data);
                        });
                }
            });        
    }


    

    function initShippingDropdown() {
        if ($scope.model.value.storeId != null) {
            $http.get('backoffice/teacommerce/shippingmethods/getall',
                {
                    params: {
                        storeId: $scope.model.value.storeId
                    }
                }
            ).success(function (data) {
                var entityFound = false;
                for (var i = 0; i < data.length; i++) {
                    //Must convert id to string because Umbraco saves and parse it back as a string
                    var entity = data[i];
                    entity.id = '' + entity.id;

                    if (entity.id == $scope.model.value.shippingMethodId) {
                        entityFound = true;
                        break;
                    }
                }

                $scope.methods = data;

                if ($scope.model.value.shippingMethodId && !entityFound) {
                    $http.get('backoffice/teacommerce/shippingmethods/get',
                        {
                            params: {
                                storeId: $scope.model.value.storeId,
                                shippingMethodId: $scope.model.value.shippingMethodId
                            }
                        }).success(function (data) {
                            data.name = '* ' + data.name;
                            $scope.methods.push(data);
                        });
                }
            });
        }
    }


    initShippingDropdown();
    initStoreDropdown();


});