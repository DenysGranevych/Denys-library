app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {

    $stateProvider
        // Admin Area 
        .state('admin', {
            aubstact: true,
            template: "<div ui-view></div>",
            resolve: {
                adminPanel: ['$ocLazyLoad', function($ocLazyLoad) {
                    return $ocLazyLoad.load('AppModule');
                }],
            }
        })
        //Login
        .state('admin.auth', {
            aubstact: true,
            template: "<div  style='background-color: #fff;' ui-view></div>",
            resolve: {
                lazyLoadModule: ['$ocLazyLoad', function($ocLazyLoad) {
                    return $ocLazyLoad.load('AuthModule');
                }],
            }
        })
        .state('admin.auth.login', {
            url: "/login",
            controller: "authController",
            templateUrl: "admin.app/views/login.html",
        })

        /// Admin Main 
        .state('admin.main', {
            aubstact: true,
            authenticate: true,
            templateUrl: "admin.app/views/main.html",
            resolve: {
                lazyLoadModule: ['$ocLazyLoad', function($ocLazyLoad) {
                    return $ocLazyLoad.load('MainModule');
                }]
            }
        })
        .state('admin.main.home', {
            url: "/home",
            templateUrl: "admin.app/views/home/index.html",
        })

        ///Kic Routing
        .state('admin.main.kic', {
            url: "/kic",
            aubstact: true,
            template: "<div ui-view> </div>",
            resolve: {
                lazyLoadModule: ['$ocLazyLoad', function($ocLazyLoad) {
                    $ocLazyLoad.load('BootstrapUiModule');
                    $ocLazyLoad.load('BaseModule');
                    $ocLazyLoad.load('SelectModule');
                    $ocLazyLoad.load('iCheckModule');
                    return $ocLazyLoad.load('KicModule');
                }]
            }
        })
        .state('admin.main.kic.index', {
            url: "/index",
            templateUrl: "admin.app/views/kic/index.html",
            controller: "kicController",
        })
        .state('admin.main.kic.edit', {
            url: "/edit?id",
            mode: 'edit',
            templateUrl: "admin.app/views/kic/edit.html",
            controller: "kicEditController",
        })
        .state('admin.main.kic.create', {
            url: "/create",
            mode: 'create',
            templateUrl: "admin.app/views/kic/edit.html",
            controller: "kicEditController",
        })
        ///Rule Routing
        .state('admin.main.rule', {
            url: "/rule",
            aubstact: true,
            template: "<div ui-view> </div>",
            resolve: {
                lazyLoadModule: ['$ocLazyLoad', function($ocLazyLoad) {
                    $ocLazyLoad.load('BootstrapUiModule');
                    $ocLazyLoad.load('BaseModule');
                    $ocLazyLoad.load('admin.app/services/kicService.min.js');
                    return $ocLazyLoad.load('RuleModule');
                }]
            }
        })
        .state('admin.main.rule.index', {
            url: "/index",
            templateUrl: "admin.app/views/rule/index.html",
            controller: "ruleController",
        })
        .state('admin.main.rule.edit', {
            url: "/edit?id&type",
            mode: 'edit',
            templateUrl: "admin.app/views/rule/edit.html",
            controller: "ruleEditController",
            resolve: {
                lazyLoadModule: ['$ocLazyLoad', function($ocLazyLoad) {
                    $ocLazyLoad.load('ClockPickerModule');
                    $ocLazyLoad.load('DatePickerModule');
                  return   $ocLazyLoad.load('iCheckModule');
                }]
            }
        })
        .state('admin.main.rule.create', {
            url: "/create?type",
            mode: 'create',
            templateUrl: "admin.app/views/rule/edit.html",
            controller: "ruleEditController",
            resolve: {
                lazyLoadModule: ['$ocLazyLoad', function ($ocLazyLoad) {
                    $ocLazyLoad.load('ClockPickerModule');
                    $ocLazyLoad.load('admin.app/services/kicService.min.js');
                    $ocLazyLoad.load('DatePickerModule');
                    return $ocLazyLoad.load('iCheckModule');
                }]
            }
        });

    $urlRouterProvider.otherwise("/home");

}])
 .run(['$rootScope', '$state', 'authService', function ($rootScope, $state, authService) {
     $rootScope['$state'] = $state;
     $rootScope['authService'] = authService;

     function $$parentState(state) {
         // Check if state has explicit parent OR we try guess parent from its name
         var name = state.parent || (/^(.+)\.[^.]+$/.exec(state.name) || [])[1];
         // If we were able to figure out parent name then get this state
         return name && $state.get(name);
     }

     $rootScope.$on("$stateChangeStart", function (event, toState) {


         for (var state = toState; state && state.name !== ''; state = $$parentState(state)) {
             if (state.authenticate && (!authService.authentication || !authService.authentication.isAuthorized)) {
                 $state.go("admin.auth.login");
                 event.preventDefault();
                 return;
             }
         }
         console.log('------------------------- Start --------------------')
         console.log('$stateChangeSuccess ' + ' Date' + new Date().toString());

     });

     $rootScope.$on("$stateChangeSuccess", function (event, toState, toParams, fromState, fromParams) {

         console.log('$stateChangeSuccess ' + ' Date' + new Date().toString());
     });
     $rootScope
         .$on('$stateChangeError',
             function (event, toState, toParams, fromState, fromParams, error) {
                 console.log('$stateChangeError ' + error);
             });
     $rootScope
         .$on('$stateNotFound',
             function (event, toState, toParams, fromState, fromParams) {
                 console.log('$stateNotFound' + ' Date' + Date.now().toString());
             });
     $rootScope
         .$on('$viewContentLoading',
             function (event, viewConfig) {
                 console.log('$viewContentLoading' + ' Date' + new Date().toString());
             });
     $rootScope
         .$on('$viewContentLoaded',
             function (event, viewConfig) {
                 console.log('$viewContentLoaded' + ' Date' + new Date().toString());
                 console.log('------------------------- End --------------------')
                 //  $rootScope.$state = $state;
             });
 }])