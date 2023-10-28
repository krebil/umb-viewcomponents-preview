angular.module("umbraco").controller("preview.controller", function ($scope, editorState, $http) {
    
    this.vm = this;

    let currentState = editorState.getCurrent();
    let block = $scope.block
    this.vm.block = block;
    let propertyController = GetUmbPropertyController($scope)
    let property = propertyController.property;

    let propertyAlias = property.alias;
    let editorAlias = property.editor;

    function GetUmbPropertyController(currentScope) {
        if(currentScope.umbProperty){
            return currentScope.umbProperty;
        }
        return GetUmbPropertyController(currentScope.$parent);
    }


    let url = "/umbraco/backoffice/api/preview/GetPreview";
    $scope.setPreview = async (block) => {
        console.log("setPreview", block)
        let value = {
            layout: {},
            contentData: property.value.contentData.filter(c => c.udi == block.data?.udi),
            settingsData: property.value.settingsData.filter(c => c.udi == block.settingsData?.udi)
        };
        value.layout[editorAlias] = [{
            'contentUdi': block.data?.udi,
            'settingsUdi': block.settingsData?.udi
        }];

        let response = await fetch(url, {
            method: 'POST',
            headers: {
                'Accept': 'application/json, text/plain',
                'Content-Type': 'application/json;charset=UTF-8'
            },
            body:JSON.stringify({
                pageId: currentState.id,
                propertyAlias: propertyAlias,
                contentTypeAlias: currentState.contentTypeAlias,
                value: JSON.stringify(value)
            })
        })
      
        let htmlResult = await response.text();
        if (htmlResult.trim().length > 0) {
            this.vm.html = htmlResult;
        }
    };


    this.vm.$onInit = () => {
        console.log("onInit", block)
        $scope.$watch('block.data', function (newVal, oldVal) {
            $scope.setPreview($scope.block);
        }, true);

        $scope.$watch('block.settingsData', function (newVal, oldVal) {
            $scope.setPreview($scope.block);
        }, true);
    }
    
  


});