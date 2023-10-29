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
        let content =  this.removeBlockReference(Object.assign({}, block.data));
        var settings = Object.assign({}, block.layout);
        delete settings.$block
        let value = {
            layout: {},
            contentData: [block.data],
            settingsData: [settings]
        };
        value.layout[editorAlias] = [settings];

        console.log(value)
        
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
    this.removeBlockReference = (content) => {
        Object.keys(content).forEach(key => {
            if(key.startsWith("$"))
            {
                delete content[key]
            }
            else if(!Array.isArray(content[key]) && Object(content[key]) === content[key])
            {
                content[key] = this.removeBlockReference(content[key])
            }
            else if(Array.isArray(content[key]) || Object(content[key]) === content[key]){
                content[key] = this.removeBlockReferenceFromArray(content[key])
            }
        })
        return content;
    }
    this.removeBlockReferenceFromArray = (contentArr) => {
        contentArr.forEach(item => {
            if(Object(item) === item){
                this.removeBlockReference(item)
            }
        })
        return contentArr;
    }

    this.vm.$onInit = () => {
        let init = 0;
        $scope.$watch('block.data', function (newVal, oldVal) {
            if(init > 0)
                $scope.setPreview($scope.block);
            init++;
        }, true);

        $scope.$watch('block.settingsData', function (newVal, oldVal) {
            if(init > 0)
                $scope.setPreview($scope.block);
            init++;
        }, true);
    }
    
  


});