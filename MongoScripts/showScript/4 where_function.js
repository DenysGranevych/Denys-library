db.getCollection('labelingv3qa').find({
    $where: function() {
        return this.labels.length > 4
    }
})
//$where will not work inside a nested document, for instance, in an $elemMatch query.
//cannot take advantage of indexes