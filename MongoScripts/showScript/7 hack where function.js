db.getCollection('labelingv3qa').find({
        $where: function() {
            var flag = true;
            for (var i = 0; i < this.labels.length; i++) {
                var existnumOfSpeakers = this.labels[i].numOfSpeakers != undefined;
                if (existnumOfSpeakers == false) {
                    flag = false
                }
            }
            return (this.labels.length > 4 && flag)
        }
    })
    //$where function is slow, and for .Net query need save in js