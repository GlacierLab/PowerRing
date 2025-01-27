const ext = ["xaml", "vb", "cs"];

const fs = require('fs');
const path = require("path");
const dirname = path.resolve("./");

let result = [];

const walk = function (dir) {
    let results = [];
    let list = fs.readdirSync(dir)
    let i = 0;
    (function next() {
        var file = list[i++];
        if (!file) return results;
        file = path.resolve(dir, file);
        let stat = fs.statSync(file)
        if (stat && stat.isDirectory()) {
            if (file.indexOf("obj") >= 0 || file.indexOf("bin") >= 0) {
            } else {
                results = results.concat(walk(file));
            }
            next();
        } else {
            let filename = path.basename(file);
            if (ext.indexOf(filename.substring(filename.lastIndexOf(".") + 1)) >= 0) {
                results.push(path.resolve(file));
            }
            next();
        }
    })();
    return results;
};
let arr = walk(dirname);
arr.forEach(item => {
    console.log(item);
    if (item.indexOf("Designer.vb") >= 0) {
        return;
    }
    let content = fs.readFileSync(item, 'utf8');
    if (content.indexOf("auto-generated") >= 0) {
        return;
    }
    Array.from(content).forEach(item => {
        if (result.indexOf(item) < 0) {
            result.push(item);
        }
    });
});
result = result.sort();
console.log("Result: " + result.join(""));
fs.writeFileSync("XiaolaiSC.txt", result.join(""));