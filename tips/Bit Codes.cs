      var list = JsonConvert.SerializeObject(comments,
                 Formatting.None,
                 new JsonSerializerSettings()
                {
                     ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore

                 });
            var jsonResult = Json(list, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

Where(l => l.Courses.Select(c => c.CourseId).Contains(courseId)


HttpRuntimeSection rt = new HttpRuntimeSection();
            rt.MaxRequestLength = rt.MaxRequestLength * 10;
            int length = rt.MaxRequestLength;
            //execution timeout
            rt.ExecutionTimeout = new TimeSpan(0, 0, 4);// five minute