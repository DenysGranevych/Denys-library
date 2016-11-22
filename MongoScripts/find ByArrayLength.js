db.getCollection('labelingv3prod').find({
    $where: "this.labels.length > 1"
})